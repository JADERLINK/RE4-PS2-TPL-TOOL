﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace TPL_PS2_REPACK
{
    public class TplImageHeader
    {
        public ushort Width;
        public ushort Height;
        public ushort BitDepth;
        public ushort Interlace;
        public ushort Next;
        public ushort MipmapStatus;
        public ushort Qwc;
        public ushort Ref;

        public uint MipmapHeader1Offset;
        public uint MipmapHeader2Offset;
        public ulong GsMip;

        public uint IndexesOffset;
        public uint PaletteOffset;
        public ulong GsTex;

        public TplImageHeader MipmapHeader1 = null;
        public TplImageHeader MipmapHeader2 = null;
    }
}
