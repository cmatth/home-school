using System;
using System.Collections.Generic;

namespace program1
{
    class Program
    {
        static void Main()
        {
            string[] input  = System.IO.File.ReadAllLines(@"MachineInstructions.txt");
            string[] output = disassembleMIPS(input);
            System.IO.File.WriteAllLines(@"Program1.txt", output);
        }

        static public string[] disassembleMIPS(string[] program)
        {
            // Only Supports instructions present in example.
            string[] opCodes = new string[44];
            opCodes[0]  = "syscall";
            opCodes[2]  = "j";
            opCodes[4]  = "beq";
            opCodes[8]  = "addi";
            opCodes[13] = "ori";
            opCodes[15] = "lui";
            opCodes[35] = "lw";
            opCodes[43] = "sw";
            
            string[] registers = { "$0", "$at", "$v0", "$v1", "$a0", "$a1", "$a2", "$a3", "$t0", "$t1", "$t2", "$t3" };
            string[] decoded = new string[program.Length];
        
            for(int i = 0; i < program.Length; i++)
            {
                string[]   input = program[i].Split('\t');
                uint addr = Convert.ToUInt32(input[0],16);
                uint inst = Convert.ToUInt32(input[1],16);

                uint funCode = inst >> 26;
                uint rs = (inst >> 21) & 0x1F;
                uint rt = (inst >> 16) & 0x1F;     
                int imm = (short)(inst & 0xFFFF);   
                uint jAddr = (inst & 0x03FFFFFF) << 2;

                switch(funCode)
                {
                    case 0: //syscall
                        decoded[i] = String.Format("0x{0:x08}\t{1}", addr, opCodes[0]);
                        break;
                    case 2: //j
                        decoded[i] = String.Format("0x{0:x08}\t{1}      0x{2:x08}", addr, opCodes[2], jAddr);
                        break;
                    case 4://beq
                        decoded[i] = String.Format("0x{0:x08}\t{1}    {2}, {3}, {4}", addr, opCodes[4], registers[rt], registers[rs], imm);
                        break;
                    case 8://addi
                        decoded[i] = String.Format("0x{0:x08}\t{1}   {2}, {3}, {4}", addr, opCodes[8], registers[rt], registers[rs], imm);
                        break;
                    case 13://ori
                        decoded[i] = String.Format("0x{0:x08}\t{1}    {2}, {3}, {4}", addr, opCodes[13], registers[rt], registers[rs], imm);
                        break;
                    case 15://lui
                        decoded[i] = String.Format("0x{0:x08}\t{1}    {2}, {3}", addr, opCodes[15], registers[rt], imm);
                        break;
                    case 35://lw
                        decoded[i] = String.Format("0x{0:x08}\t{1}     {2}, {3}({4})", addr, opCodes[35], registers[rt], imm, registers[rs]);
                        break;
                    case 43://sw
                        decoded[i] = String.Format("0x{0:x08}\t{1}     {2}, {3}({4})", addr, opCodes[43], registers[rt], imm, registers[rs]);
                        break;
                    default:
                        decoded[i] = "Instruction not supported.";
                        break;
                }
            }
            return decoded;
        }
    }

    class MemoryModel
    {
        private uint BaseAddress;
        private int[] MemorySpace;

        public MemoryModel(uint _baseAddress, int _size)
        {
            BaseAddress = _baseAddress;
            MemorySpace = new int[_size];
        }

        public int Load(uint address)
        {
            return MemorySpace[(address - BaseAddress) >> 2];
        }

        public void Store(uint address, int data)
        {
            MemorySpace[(address - BaseAddress) >> 2] = data;
        }

        public void Dump()
        {
            string[] memoryData = new string[MemorySpace.Length];
            for(int i=0; i < MemorySpace.Length; i++)
            {
                memoryData[i] = String.Format("0x{0:08x}\t0x{1:08x}", (i << 2) + BaseAddress, MemorySpace[i]);
            }
            System.IO.File.WriteAllLines(@"MemoryDump.txt", memoryData);
        }
    }
}
