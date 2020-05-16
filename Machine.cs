using System;
using System.Collections.Generic;
using System.Text;

namespace CHROMA_8
{
    public class Machine
    {
        public readonly static byte[] FONTSET = new byte[80] {
            0xF0, 0x90, 0x90, 0x90, 0xF0, //0
            0x20, 0x60, 0x20, 0x20, 0x70, //1
            0xF0, 0x10, 0xF0, 0x80, 0xF0, //2
            0xF0, 0x10, 0xF0, 0x10, 0xF0, //3
            0x90, 0x90, 0xF0, 0x10, 0x10, //4
            0xF0, 0x80, 0xF0, 0x10, 0xF0, //5
            0xF0, 0x80, 0xF0, 0x90, 0xF0, //6
            0xF0, 0x10, 0x20, 0x40, 0x40, //7
            0xF0, 0x90, 0xF0, 0x90, 0xF0, //8
            0xF0, 0x90, 0xF0, 0x10, 0xF0, //9
            0xF0, 0x90, 0xF0, 0x90, 0x90, //A
            0xE0, 0x90, 0xE0, 0x90, 0xE0, //B
            0xF0, 0x80, 0x80, 0x80, 0xF0, //C
            0xE0, 0x90, 0x90, 0x90, 0xE0, //D
            0xF0, 0x80, 0xF0, 0x80, 0xF0, //E
            0xF0, 0x80, 0xF0, 0x80, 0x80  //F
        };

        public byte[] Memory = new byte[0x1000];
        public byte[] V = new byte[16];
        public bool[,] Display = new bool[64,32];

        public byte DelayTimer = 0;
        public byte SoundTimer = 0;

        private ushort I = 0;
        private ushort PC = 0x200;
        private byte SP = 0;
        private ushort[] Stack = new ushort[16];

        private bool[] Input = new bool[16];

        public bool ScreenUpdate = false;
        public bool WaitForInput = false;

        private byte TimerCountdown = 0;

        private Random rnd = new Random();

        public Machine(byte[] rom)
        {
            // Load ROM
            for(int i = 0; i < rom.Length ; i++)
            {
                if (i + 0x200 > 0xFFF) break;
                Memory[i + 0x200] = rom[i];
            }

            // Load Fontset
            for (int i = 0; i < FONTSET.Length; i++)
                Memory[i] = FONTSET[i];
        }

        public void HandleInput(int value, bool state)
        {
            Input[value] = state;
            if (state && WaitForInput) WaitForInput = false;
        }

        public void EmulateCycle()
        {
            if (WaitForInput)
                return;

            ushort opcode = (ushort)((Memory[PC++] << 8) + Memory[PC++]);

            switch((opcode & 0xF000))
            {
                case 0x0000:
                    if(opcode == 0xE0) { // CLS
                        lock(Display)
                        {
                            Display = new bool[64, 32];
                            ScreenUpdate = true;
                        }
                    } else if(opcode == 0xEE) // RET
                        PC = (ushort)(Stack[SP--]);
                    break;
                case 0x1000: // JP addr
                    PC = (ushort)(opcode & 0xFFF);
                    break;
                case 0x2000: // CALL addr
                    Stack[++SP] = PC;
                    PC = (ushort)(opcode & 0xFFF);
                    break;
                case 0x3000: // SE Vx, byte
                    if (V[(opcode & 0xF00) >> 8] == (opcode & 0xFF))
                        PC += 2;
                    break;
                case 0x4000: // SNE Vx, byte
                    if (V[(opcode & 0xF00) >> 8] != (opcode & 0xFF))
                        PC += 2;
                    break;
                case 0x5000: // SE Vx, Vy
                    if (V[(opcode & 0xF00) >> 8] == V[(opcode & 0xF0) >> 4])
                        PC += 2;
                    break;
                case 0x6000: // LD Vx, byte
                    V[(opcode & 0xF00) >> 8] = (byte)(opcode & 0xFF);
                    break;
                case 0x7000: // ADD Vx, byte
                    V[(opcode & 0xF00) >> 8] += (byte)(opcode & 0xFF);
                    break;
                case 0x8000:
                    switch(opcode & 0xF)
                    {
                        case 0x0: // LD Vx, Vy
                            V[(opcode & 0xF00) >> 8] = V[(opcode & 0xF0) >> 4];
                            break;
                        case 0x1: // OR Vx, Vy
                            V[(opcode & 0xF00) >> 8] |= V[(opcode & 0xF0) >> 4];
                            break;
                        case 0x2: // AND Vx, Vy
                            V[(opcode & 0xF00) >> 8] &= V[(opcode & 0xF0) >> 4];
                            break;
                        case 0x3: // XOR Vx, Vy
                            V[(opcode & 0xF00) >> 8] ^= V[(opcode & 0xF0) >> 4];
                            break;
                        case 0x4: // ADD Vx, Vy
                            if (V[(opcode & 0xF00) >> 8] + V[(opcode & 0xF0) >> 4] > 255) V[0xF] = 1;
                            else V[0xF] = 0;
                            V[(opcode & 0xF00) >> 8] += V[(opcode & 0xF0) >> 4];
                            break;
                        case 0x5: // SUB Vx, Vy
                            if (V[(opcode & 0xF00) >> 8] > V[(opcode & 0xF0) >> 4]) V[0xF] = 1;
                            else V[0xF] = 0;
                            V[(opcode & 0xF00) >> 8] -= V[(opcode & 0xF0) >> 4];
                            break;
                        case 0x6: // SHR Vx
                            V[0xF] = (byte)(V[(opcode & 0xF00) >> 8] & 1);
                            V[(opcode & 0xF00) >> 8] >>= 1;
                            break;
                        case 0x7: // SUBN Vx, Vy
                            if (V[(opcode & 0xF00) >> 8] < V[(opcode & 0xF0) >> 4]) V[0xF] = 1;
                            else V[0xF] = 0;
                            V[(opcode & 0xF00) >> 8] -= V[(opcode & 0xF0) >> 4];
                            break;
                        case 0xE: // SHL Vx
                            V[0xF] = (byte)((V[(opcode & 0xF00) >> 8] & 128) >> 7);
                            V[(opcode & 0xF00) >> 8] <<= 1;
                            break;
                    }
                    break;
                case 0x9000: // SNE Vx, Vy
                    if (V[(opcode & 0xF00) >> 8] != V[(opcode & 0xF0) >> 4])
                        PC += 2;
                    break;
                case 0xA000: // LD I, addr
                    I = (ushort)(opcode & 0xFFF);
                    break;
                case 0xB000: // JP V0, addr
                    PC = (ushort)((opcode & 0xFFF) + V[0]);
                    break;
                case 0xC000: // RND Vx, byte
                    V[(opcode & 0xF00) >> 8] = (byte)((opcode & 0xFF) & (byte)rnd.Next(0, 255));
                    break;
                case 0xD000: // DRW Vx, Vy, nibble
                    int x = V[(opcode & 0xF00) >> 8];
                    int y = V[(opcode & 0xF0) >> 4];
                    bool coll = false;
                    lock (Display)
                    {
                        for (int i = I, b = 0; b < (opcode & 0xF); i++, b++)
                        {
                            for (int bit = 0, bitmap = 128; bit < 8; bit++, bitmap >>= 1)
                            {
                                bool cb = (Memory[i] & bitmap) > 0;
                                if (Display[(x + bit) % 64, (y + b) % 32] && cb) coll = true;
                                Display[(x + bit) % 64, (y + b) % 32] ^= cb;
                            }
                        }
                        if (coll) V[0xF] = 1;
                        else V[0xF] = 0;
                    }
                    ScreenUpdate = true;
                    break;
                case 0xE000:
                    switch(opcode & 0xF)
                    {
                        case 0xE: // SKP Vx
                            if (Input[V[(opcode & 0xF00) >> 8]]) PC += 2;
                            break;
                        case 0x1: // SKNP Vx
                            if (!Input[V[(opcode & 0xF00) >> 8]]) PC += 2;
                            break;
                    }
                    break;
                case 0xF000:
                    switch(opcode & 0xFF)
                    {
                        case 0x7: // LD Vx, DT
                            V[(opcode & 0xF00) >> 8] = DelayTimer;
                            break;
                        case 0x0A: // LD Vx, K
                            WaitForInput = true;
                            return;
                        case 0x15: // LD DT, Vx
                            DelayTimer = V[(opcode & 0xF00) >> 8];
                            break;
                        case 0x18: // LD ST, Vx
                            SoundTimer = V[(opcode & 0xF00) >> 8];
                            break;
                        case 0x1E: // ADD I, Vx
                            if (I + V[(opcode & 0x0F00) >> 8] > 0xFFF)
                                V[0xF] = 1;
                            else
                                V[0xF] = 0;
                            I += V[(opcode & 0xF00) >> 8];
                            break;
                        case 0x29: // LD F, Vx
                            I = (ushort)(5 * V[(opcode & 0xF00) >> 8]);
                            break;
                        case 0x33: // LD B, Vx
                            byte hundreds = (byte)(V[(opcode & 0xF00) >> 8] / 100);
                            byte tens = (byte)((V[(opcode & 0xF00) >> 8] % 100) / 10);
                            byte ones = (byte)(V[(opcode & 0xF00) >> 8] % 10);
                            Memory[I] = hundreds;
                            Memory[I + 1] = tens;
                            Memory[I + 2] = ones;
                            break;
                        case 0x55: // LD [I], Vx
                            for (int i = 0; i <= ((opcode & 0xF00) >> 8); i++)
                                Memory[I + i] = V[i];
                            break;
                        case 0x65: // LD [I], Vx
                            for (int i = 0; i <= ((opcode & 0xF00) >> 8); i++)
                                V[i] = Memory[I + i];
                            break;
                    }
                    break;
                default:
                    throw new NotImplementedException("Unknown opcode: 0x" + opcode.ToString("X4"));
            }

            if (TimerCountdown == 0)
            {
                if (DelayTimer > 0) DelayTimer--;
                if (SoundTimer > 0) SoundTimer--;
                TimerCountdown = 4;
            }
            else TimerCountdown--;

            // DebugLog();
        }

        private void DebugLog()
        {
            Console.WriteLine("==========================================================");
            Console.WriteLine("=                      DEBUG LOG                         =");
            Console.WriteLine("==========================================================");
            Console.WriteLine("PC = " + PC.ToString("X4"));
            Console.WriteLine("SP = " + SP.ToString("X4"));
            for (int i = 0; i < V.Length; i++)
                Console.WriteLine("V[" + i.ToString("X") + "] = " + V[i].ToString("X2"));
            Console.WriteLine("I = " + I.ToString("X4"));
        }
    }
}
