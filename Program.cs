using System;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Writer;
using OpCodes = dnlib.DotNet.Emit.OpCodes;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("ArmDot patcher by t.me/dotnetreverse\n");
        string path = "";
        if (args.Length > 0)
        {
            path = args[0];
            if (!File.Exists(path))
            {
                Console.Write("Enter path to ArmDot.Engine.dll:");
                path = Console.ReadLine();

                if (!File.Exists(path))
                {
                    Console.WriteLine("Enter valid path!");
                    Console.ReadKey();
                    return;
                }
            }
        }
        else
        {
            Console.Write("Enter path to ArmDot.Engine.dll: ");
            path = Console.ReadLine();

            if (!File.Exists(path))
            {
                Console.WriteLine("Enter valid path!");
                Console.ReadKey();
                return;
            }

        }


        var module = ModuleDefMD.Load(path);
        bool patched = false;

        foreach (var type in module.GetTypes().Where(t => t.Namespace == "ArmDot.Engine.CodeConverters.HighLevel"))
        {
            foreach (var method in type.Methods)
            {
                if (!method.HasBody)
                {
                    continue;
                }

                var instr = method.Body.Instructions;

                if (instr.Count == 2 && instr[0].OpCode == OpCodes.Call && instr[0].Operand is IMethod methodRef &&
                    methodRef.FullName.Contains("System.DateTime::get_UtcNow") &&
                    instr[1].OpCode == OpCodes.Ret)
                {
                    Console.WriteLine($"Patching: {method.Name}");
                    instr.Clear();
                    method.Body.KeepOldMaxStack = true;
                    var ticks = new DateTime(3000, 1, 1).Ticks;
                    var dateTimeType = module.CorLibTypes.GetTypeRef("System", "DateTime");
                    var ctorSig = MethodSig.CreateInstance(module.CorLibTypes.Void, module.CorLibTypes.Int64);
                    var ctor = new MemberRefUser(module, ".ctor", ctorSig, dateTimeType);

                    instr.Add(OpCodes.Ldc_I8.ToInstruction(ticks));
                    instr.Add(OpCodes.Newobj.ToInstruction(ctor));
                    instr.Add(OpCodes.Ret.ToInstruction());

                    patched = true;
                }
            }
        }

        string outputPath = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path) + "_patched.dll");

        if (patched)
        {
            var writerOptions = new ModuleWriterOptions(module);
            writerOptions.MetadataOptions.Flags |= MetadataFlags.KeepOldMaxStack;

            module.Write(outputPath, writerOptions);
            Console.WriteLine("Patched.");
            Console.ReadKey();
        }
        else
        {
            Console.WriteLine("Not found namespace");
        }
    }
}
