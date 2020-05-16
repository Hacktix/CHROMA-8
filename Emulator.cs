using Chroma;
using Chroma.Graphics;
using Chroma.Input;
using Chroma.Input.EventArgs;
using ChromaSynth;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace CHROMA_8
{
    public class Emulator : Game
    {
        private static readonly int SCREEN_WIDTH = 64;
        private static readonly int SCREEN_HEIGHT = 32;

        private static readonly Dictionary<KeyCode, int> KEYMAP = new Dictionary<KeyCode, int>()
        {
            { KeyCode.Alpha1, 0x1 },
            { KeyCode.Alpha2, 0x2 },
            { KeyCode.Alpha3, 0x3 },
            { KeyCode.Alpha4, 0xC },
            { KeyCode.Q, 0x4 },
            { KeyCode.W, 0x5 },
            { KeyCode.E, 0x6 },
            { KeyCode.R, 0xD },
            { KeyCode.A, 0x7 },
            { KeyCode.S, 0x8 },
            { KeyCode.D, 0x9 },
            { KeyCode.F, 0xE },
            { KeyCode.Z, 0xA },
            { KeyCode.X, 0x0 },
            { KeyCode.C, 0xB },
            { KeyCode.V, 0xF },
        };

        private Waveform AudioWave;
        private bool playingSound = false;
        private int scaleFactor = 12;
        private Machine machine;

        private RenderTarget _tgt;

        public Emulator(byte[] rom)
        {
            machine = new Machine(rom);

            Graphics.VSyncEnabled = false;
            Graphics.AutoClear = false;
            _tgt = new RenderTarget((ushort)(SCREEN_WIDTH * scaleFactor), (ushort)(SCREEN_HEIGHT * scaleFactor));

            Window.Properties.Title = "CHROMA-8";
            FixedUpdateFrequency = 1000;
            AudioWave = new SineWave(Audio, 220);

            Window.GoWindowed((ushort)(SCREEN_WIDTH * scaleFactor), (ushort)(SCREEN_HEIGHT * scaleFactor));
        }

        protected override void FixedUpdate(float fixedDelta)
        {
            Window.Properties.Title = "CHROMA-8 (" + Window.FPS + " FPS)";
            machine.EmulateCycle();

            if (!playingSound && machine.SoundTimer > 0)
                EnableSound();
            else if (playingSound && machine.SoundTimer == 0)
                DisableSound();
        }

        protected override void Draw(RenderContext context)
        {
            if(machine.ScreenUpdate)
            {
                context.RenderTo(_tgt, () =>
                {
                    context.Clear(Color.Black);
                    lock(machine.Display)
                    {
                        for (int x = 0; x < SCREEN_WIDTH; x++)
                        {
                            for (int y = 0; y < SCREEN_HEIGHT; y++)
                            {
                                if (machine.Display[x, y])
                                    context.Rectangle(ShapeMode.Fill, new Vector2(x * scaleFactor, y * scaleFactor), scaleFactor, scaleFactor, Color.White);
                            }
                        }
                    }
                });
                machine.ScreenUpdate = false;
                
            }
            context.DrawTexture(_tgt, Vector2.Zero, Vector2.One, Vector2.Zero, .0f);
        }

        protected override void KeyPressed(KeyEventArgs e)
        {
            if(KEYMAP.ContainsKey(e.KeyCode))
                machine.HandleInput(KEYMAP[e.KeyCode], true);
        }

        protected override void KeyReleased(KeyEventArgs e)
        {
            if (KEYMAP.ContainsKey(e.KeyCode))
                machine.HandleInput(KEYMAP[e.KeyCode], false);
        }

        private void EnableSound()
        {
            playingSound = true;
            Audio.HookPostMixProcessor<float>((chunk, bytes) => { AudioWave.GenerateChunk(ref chunk); });
        }

        private void DisableSound()
        {
            playingSound = false;
            Audio.UnhookPostMixProcessor();
        }
    }
}
