using System;
using System.Collections.Generic;

namespace program1
{
    class Program
    {
        static void Main()
        {
            string[] input  = System.IO.File.ReadAllLines(@"MachineInstructions.txt");
            executeMIPS(input);
        }

        static public void executeMIPS(string[] program)
        {
            uint PC = Convert.ToUInt32(program[0].Split('\t')[0],16);
            int[] registers = new int[12];
            bool halt = false; //syscall sets to true
            int cycleCount = 0;

            MemoryModel IM = new MemoryModel(PC, program.Length);
            for(uint i = 0; i < program.Length; i++)
            {
                IM.Store(PC + (i * 4), Convert.ToUInt32(program[i].Split('\t')[1],16));
            }

            MemoryModel DM = new MemoryModel(0x10010000u, 9);
            for(uint i = 0; i < 9; i++)
            {
                DM.Store(0x10010000u + (i * 4), i);
            }
            DM.Dump(@"DMDumpBefore.txt");

            while(!halt)
            {
                cycleCount++;
                uint inst = IM.Load(PC);
                PC += 4;

                uint funCode = inst >> 26;
                uint rs = (inst >> 21) & 0x1F;
                uint rt = (inst >> 16) & 0x1F;     
                int imm = (short)inst & 0xFFFF;   
                uint jAddr = (inst & 0x03FFFFFF) << 2;
                uint func = inst & 0x1F;

                switch(funCode)
                {
                    case 0: //syscall   
                        if(func == 12) halt = true;
                        break;
                    case 2: //j
                        PC = jAddr;
                        break;
                    case 4://beq
                        if(registers[rs] == registers[rt]) PC += Convert.ToUInt32(imm << 2);
                        break;
                    case 8://addi
                        registers[rt] = registers[rs] + imm;
                        break;
                    case 13://ori
                        registers[rt] = registers[rs] ^ imm;
                        break;
                    case 15://lui
                        registers[rt] = imm << 16;
                        break;
                    case 35://lw
                        registers[rt] = Convert.ToInt32(DM.Load(Convert.ToUInt32(registers[rs] + imm)));
                        break;
                    case 43://sw
                        DM.Store(Convert.ToUInt32(registers[rs] + imm), Convert.ToUInt32(registers[rt]));
                        break;
                    default:
                        break;
                }
            }
            IM.Dump(@"IMDump.txt");
            DM.Dump(@"DMDumpAfter.txt");
        }
    }

    class MemoryModel
    {
        private uint BaseAddress;
        private uint[] MemorySpace;

        public MemoryModel(uint _baseAddress, int _size)
        {
            BaseAddress = _baseAddress;
            MemorySpace = new uint[_size];
        }
        public uint Load(uint address)
        {
            return MemorySpace[(address - BaseAddress) >> 2];
        }
        public void Store(uint address, uint data)
        {
            MemorySpace[(address - BaseAddress) >> 2] = data;
        }

        public void Dump(string filename)
        {
            string[] memoryData = new string[MemorySpace.Length];
            for(int i=0; i < MemorySpace.Length; i++)
            {
                memoryData[i] = String.Format("0x{0:x08}\t0x{1:x08}", (i << 2) + BaseAddress, MemorySpace[i]);
            }
            System.IO.File.WriteAllLines(@filename, memoryData);
        }
    }
}
