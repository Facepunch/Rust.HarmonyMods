using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Facepunch.Harmony.Weaver
{
    internal abstract class BaseTranspileHook
    {
        private ILGenerator ilGen;
        private MethodBase OriginalMethod;
        protected List<LocalVariableInfo> LocalVariables;

        #region Inject

        public void CallMethod( MethodInfo info )
        {
            Inject( OpCodes.Call, info );
        }

        public void CallHookMethod( Type type )
        {
            MethodInfo method = type.GetMethod( "Hook", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public );

            if ( method == null )
            {
                throw new Exception( $"Failed to find \"Hook\" method in '{type.Name}'" );
            }

            Inject( OpCodes.Call, method );

            // No need to pop if its a void method
            if ( method.ReturnType != typeof( void ) )
            {
                // If the method returns a bool, continue on true and return on false
                if ( method.ReturnType == typeof( bool ) )
                {
                    ReturnIfBool( false );
                }
                else
                {
                    // Any other return type gets ignored until supported in some way
                    Inject( OpCodes.Pop );
                }
            }
        }

        public void ReturnIfBool( bool returnState )
        {
            var nextInstruction = PeekAhead();
            var continueExecutionLabel = ilGen.DefineLabel();
            nextInstruction.labels.Add( continueExecutionLabel );

            // Assuming bool we checking is currently on the stack
            Inject( returnState ? OpCodes.Brfalse : OpCodes.Brtrue, continueExecutionLabel );
            Inject( OpCodes.Ret );
        }

        #endregion

        #region ILCode Helpers

        public void ReturnIfNotNull()
        {
            Inject( OpCodes.Ldnull );
            var postHookLabel = ilGen.DefineLabel();
            PeekAhead().labels.Add( postHookLabel );
            Inject( OpCodes.Beq_S, postHookLabel );
            Inject( OpCodes.Ret );
        }

        protected void LoadThis()
        {
            Inject( OpCodes.Ldarg_0 );
        }

        protected void LoadParameter( int index )
        {
            //Add 1 to argument index when instance method because arg0 = this
            if ( OriginalMethod.IsStatic == false )
            {
                index++;
            }
            switch ( index )
            {
                case 0:
                    {
                        Inject( OpCodes.Ldarg_0 );
                        return;
                    }
                case 1:
                    {
                        Inject( OpCodes.Ldarg_1 );
                        return;
                    }
                case 2:
                    {
                        Inject( OpCodes.Ldarg_2 );
                        return;
                    }
                case 3:
                    {
                        Inject( OpCodes.Ldarg_3 );
                        return;
                    }
                default:
                    {
                        Inject( OpCodes.Ldarg_S, index );
                        return;
                    }
            }
        }

        protected void LoadLocal( int index )
        {
            switch ( index )
            {
                case 0:
                    {
                        Inject( OpCodes.Ldloc_0 );
                        return;
                    }
                case 1:
                    {
                        Inject( OpCodes.Ldloc_1 );
                        return;
                    }
                case 2:
                    {
                        Inject( OpCodes.Ldloc_2 );
                        return;
                    }
                case 3:
                    {
                        Inject( OpCodes.Ldloc_3 );
                        return;
                    }
                default:
                    {
                        Inject( OpCodes.Ldloc_S, index );
                        return;
                    }
            }
        }

        protected void LoadBool( bool state )
        {
            if ( state )
            {
                Inject( OpCodes.Ldc_I4_1 );
            }
            else
            {
                Inject( OpCodes.Ldc_I4_0 );
            }
        }

        protected void LoadString( string text )
        {
            Inject( OpCodes.Ldstr, text );
        }

        protected void Inject( OpCode code, object operand = null )
        {
            instructions.Insert( ++currentIndex, new CodeInstruction( code, operand ) );
        }

        public bool IsMethodCall( CodeInstruction instruction )
        {
            return instruction.opcode == OpCodes.Call || instruction.opcode == OpCodes.Callvirt || instruction.opcode == OpCodes.Calli;
        }

        public int GetLocalIndex( CodeInstruction instruction )
        {
            if ( instruction.opcode == OpCodes.Ldloc_0 )
            {
                return 0;
            }
            else if ( instruction.opcode == OpCodes.Ldloc_1 )
            {
                return 1;
            }
            else if ( instruction.opcode == OpCodes.Ldloc_2 )
            {
                return 2;
            }
            else if ( instruction.opcode == OpCodes.Ldloc_3 )
            {
                return 3;
            }
            else if ( instruction.opcode == OpCodes.Ldloc_S || instruction.opcode == OpCodes.Ldloc )
            {
                return ( instruction.operand as LocalVariableInfo ).LocalIndex;
            }
            else
            {
                return -1;
            }
        }

        public int GetStackModification( CodeInstruction instruction )
        {
            //Console.WriteLine( $"Stack: {instruction} Pop: {instruction.opcode.StackBehaviourPop} Push: {instruction.opcode.StackBehaviourPush}" );

            return GetStackBehaviour( instruction.opcode.StackBehaviourPush ) + GetStackBehaviour( instruction.opcode.StackBehaviourPop );
        }

        private int GetStackBehaviour( StackBehaviour behaviour )
        {
            var name = behaviour.ToString();

            var split = behaviour.ToString().Split( '_' );

            switch ( behaviour )
            {
                case StackBehaviour.Pop0:
                case StackBehaviour.Push0:
                    {
                        return 0;
                    }
            }

            if ( name.StartsWith( "Push" ) )
            {
                return split.Length;
            }
            else if ( name.StartsWith( "Pop" ) )
            {
                return -split.Length;
            }
            else if ( behaviour == StackBehaviour.Varpop )
            {
                return -1;
            }
            else if ( behaviour == StackBehaviour.Varpush )
            {
                return 1;
            }

            return 0;
        }

        #endregion

        #region Transpiling

        protected static IEnumerable<CodeInstruction> DoTranspile( Type type, IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase originalMethod )
        {
            BaseTranspileHook instance = Activator.CreateInstance( type ) as BaseTranspileHook;

            instance.LocalVariables = originalMethod.GetMethodBody().LocalVariables.ToList();

            return instance.InternalTranspile( instructions, generator, originalMethod );
        }

        private int currentIndex;
        private List<CodeInstruction> instructions;

        public IEnumerable<CodeInstruction> InternalTranspile( IEnumerable<CodeInstruction> input, ILGenerator generator, MethodBase originalMethod )
        {
            ilGen = generator;
            OriginalMethod = originalMethod;

            instructions = input.ToList();
            for ( currentIndex = 0; currentIndex < instructions.Count; currentIndex++ )
            {
                var instruction = CurrentInstruction();

                if ( WeaveHook( instruction ) )
                {
                    return instructions;
                }
            }

            return input;
        }

        public abstract bool WeaveHook( CodeInstruction instruction );

        #endregion

        #region Move/Search Ahead and Behind

        public bool MoveBeforeMethod()
        {
            if ( !IsMethodCall( CurrentInstruction() ) )
            {
                return false;
            }

            var method = CurrentInstruction().GetMethod();

            int stackSizeLeft = -method.GetParameters().Length;

            if ( method.IsStatic == false )
            {
                stackSizeLeft--;
            }

            int i = 1;

            var instruction = PeekBehind( i );

            while ( instruction != null && stackSizeLeft != 0 )
            {
                int stackchange = GetStackModification( instruction );

                stackSizeLeft += stackchange;

                instruction = PeekBehind( ++i );
            }

            if ( stackSizeLeft == 0 )
            {
                currentIndex -= i;
                return true;
            }

            return false;
        }

        public bool MoveAfterMethod()
        {
            bool moved = IsMethodCall( CurrentInstruction() );

            if ( moved == false )
            {
                moved = MoveAfter( search => { return IsMethodCall( search ); } );
            }

            if ( moved == true )
            {
                if ( PeekAhead().opcode == OpCodes.Pop )
                {
                    MoveForward();
                }
            }

            return moved;
        }

        public bool MoveBefore( Func<CodeInstruction, bool> search )
        {
            int index = Search( search, -1 );

            if ( index == -1 )
            {
                return false;
            }

            currentIndex = index;
            return true;
        }

        public bool MoveAfter( Func<CodeInstruction, bool> search )
        {
            int index = Search( search, 1 );

            if ( index == -1 )
            {
                return false;
            }

            currentIndex = index;
            return true;
        }

        public CodeInstruction SearchBefore( Func<CodeInstruction, bool> search )
        {
            int index = Search( search, -1 );

            return index == -1 ? null : instructions[ index ];
        }

        public CodeInstruction SearchAfter( Func<CodeInstruction, bool> search )
        {
            int index = Search( search, 1 );

            return index == -1 ? null : instructions[ index ];
        }

        private CodeInstruction SearchForInstruction( SearchDirection direction, Func<CodeInstruction, bool> search )
        {
            int index = Search( search, (int)direction );

            return index == -1 ? null : instructions[ index ];
        }

        private int Search( Func<CodeInstruction, bool> search, int direction )
        {
            for ( int index = currentIndex; index < instructions.Count && index >= 0; index += direction )
            {
                if ( search( instructions[ index ] ) )
                {
                    return index;
                }
            }
            return -1;
        }

        public CodeInstruction SearchLoadLocal( SearchDirection direction, Type localType, out LocalVariableInfo local )
        {
            CodeInstruction searchResult = SearchForInstruction( direction, instruction => { return instruction.CheckLoadLocal( localType, LocalVariables ); } );

            if ( searchResult == null )
            {
                local = null;
            }
            else
            {
                local = LocalVariables[ GetLocalIndex( searchResult ) ];
            }

            return searchResult;
        }

        public CodeInstruction SearchStoreLocal( SearchDirection direction, Type localType, out LocalVariableInfo local )
        {
            CodeInstruction searchResult = SearchForInstruction( direction, instruction => { return instruction.CheckLoadLocal( localType, LocalVariables ); } );

            if ( searchResult == null )
            {
                local = null;
            }
            else
            {
                local = LocalVariables[ GetLocalIndex( searchResult ) ];
            }

            return searchResult;
        }

        #endregion

        #region Peek and Move

        public CodeInstruction PeekAhead( int amount = 1 )
        {
            return Peek( amount );
        }

        public CodeInstruction PeekBehind( int amount = 1 )
        {
            return Peek( -amount );
        }

        public CodeInstruction Peek( int amount )
        {
            return GetAtIndex( currentIndex + amount );
        }

        public CodeInstruction CurrentInstruction()
        {
            return GetAtIndex( currentIndex );
        }

        private CodeInstruction GetAtIndex( int index )
        {
            if ( index < 0 || index >= instructions.Count )
            {
                return null;
            }
            return instructions[ index ];
        }

        public bool MoveForward()
        {
            if ( currentIndex >= instructions.Count )
            {
                return false;
            }

            currentIndex++;

            return true;
        }

        public bool MoveBack()
        {
            if ( currentIndex == 0 )
            {
                return false;
            }

            currentIndex--;

            return true;
        }

        #endregion
    }

    public enum SearchDirection
    {
        After = 1,
        Before = -1,
    }

    public class ArgumentSetting
    {
        public bool This;
        public int ArgumentIndex = -1;
        public int LocalIndex = -1;
    }

    public class ReturnSetting
    {
        public bool Continue;
        public bool Exit;
    }

    public static class Arg
    {
        public static ArgumentSetting This()
        {
            return new ArgumentSetting()
            {
                This = true,
            };
        }

        public static ArgumentSetting Parameter( int index )
        {
            return new ArgumentSetting()
            {
                ArgumentIndex = index,
            };
        }

        public static ArgumentSetting Local( int index )
        {
            return new ArgumentSetting()
            {
                LocalIndex = index,
            };
        }
    }

    public static class Return
    {
        public static ReturnSetting Continue()
        {
            return new ReturnSetting()
            {
                Continue = true,
            };
        }

        public static ReturnSetting Exit()
        {
            return new ReturnSetting()
            {
                Exit = true,
            };
        }
    }

}
