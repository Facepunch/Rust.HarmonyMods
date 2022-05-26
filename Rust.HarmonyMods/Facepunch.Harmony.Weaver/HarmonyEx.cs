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
    public static class HarmonyEx
    {
        public static FieldInfo GetField( this CodeInstruction instruction )
        {
            return instruction.operand as FieldInfo;
        }

        public static MethodInfo GetMethod( this CodeInstruction instruction )
        {
            return instruction.operand as MethodInfo;
        }

        public static bool CheckField( this CodeInstruction instruction, Type classType, string fieldName, Type fieldType = null )
        {
            var field = instruction.operand as FieldInfo;
            if ( field == null )
            {
                return false;
            }

            if ( field.DeclaringType != classType )
            {
                return false;
            }

            if ( field.Name != fieldName )
            {
                return false;
            }

            if ( fieldType != null && field.FieldType != fieldType )
            {
                return false;
            }

            return true;
        }

        public static bool CheckMethod( this CodeInstruction instruction, string methodName, Type declaringType = null )
        {
            var method = instruction.operand as MethodInfo;
            if ( method == null )
            {
                return false;
            }

            if ( method.Name != methodName )
            {
                return false;
            }

            if ( declaringType != null && method.DeclaringType != declaringType )
            {
                return false;
            }

            return true;
        }

        public static bool CheckLoadLocal( this CodeInstruction instruction )
        {
            return CheckLoadLocal( instruction, null, null );
        }

        public static bool CheckLoadLocal( this CodeInstruction instruction, Type targetLocalType, List<LocalVariableInfo> localVariables )
        {
            if ( !IsLoadLocal( instruction ) )
            {
                return false;
            }

            if ( targetLocalType != null )
            {
                var local = instruction.operand as LocalVariableInfo;
                if ( local == null )
                {
                    int localIndex = -1;
                    if ( instruction.opcode == OpCodes.Ldloc_0 )
                    {
                        localIndex = 0;
                    }
                    else if ( instruction.opcode == OpCodes.Ldloc_1 )
                    {
                        localIndex = 1;
                    }
                    else if ( instruction.opcode == OpCodes.Ldloc_2 )
                    {
                        localIndex = 2;
                    }
                    else if ( instruction.opcode == OpCodes.Ldloc_3 )
                    {
                        localIndex = 3;
                    }
                    if ( localIndex != -1 && localIndex < localVariables.Count )
                    {
                        local = localVariables[ localIndex ];
                    }
                }

                if ( local == null || local.LocalType != targetLocalType )
                {
                    return false;
                }
            }

            return true;
        }

        public static bool CheckStoreLocal( this CodeInstruction instruction, Type targetLocalType, List<LocalVariableInfo> localVariables )
        {
            if ( !IsStoreLocal( instruction ) )
            {
                return false;
            }

            if ( targetLocalType != null )
            {
                var local = instruction.operand as LocalVariableInfo;
                if ( local == null )
                {
                    int localIndex = -1;
                    if ( instruction.opcode == OpCodes.Stloc_0 )
                    {
                        localIndex = 0;
                    }
                    else if ( instruction.opcode == OpCodes.Stloc_1 )
                    {
                        localIndex = 1;
                    }
                    else if ( instruction.opcode == OpCodes.Stloc_2 )
                    {
                        localIndex = 2;
                    }
                    else if ( instruction.opcode == OpCodes.Stloc_3 )
                    {
                        localIndex = 3;
                    }
                    if ( localIndex != -1 && localIndex < localVariables.Count )
                    {
                        local = localVariables[ localIndex ];
                    }
                }

                if ( local == null || local.LocalType != targetLocalType )
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsLoadLocal( this CodeInstruction instruction )
        {
            return instruction.opcode == OpCodes.Ldloc || instruction.opcode == OpCodes.Ldloc_0 || instruction.opcode == OpCodes.Ldloc_1 || instruction.opcode == OpCodes.Ldloc_2
                || instruction.opcode == OpCodes.Ldloc_3 || instruction.opcode == OpCodes.Ldloc_S;
        }

        public static bool IsStoreLocal( this CodeInstruction instruction )
        {
            return instruction.opcode == OpCodes.Stloc || instruction.opcode == OpCodes.Stloc_0 || instruction.opcode == OpCodes.Stloc_1 || instruction.opcode == OpCodes.Stloc_2
                || instruction.opcode == OpCodes.Stloc_3 || instruction.opcode == OpCodes.Stloc_S;
        }
    }
}
