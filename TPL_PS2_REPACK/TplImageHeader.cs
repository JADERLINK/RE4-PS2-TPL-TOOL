using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace TPL_PS2_REPACK
{
    public class TplImageHeader
    {

        public ushort width;
        public ushort height;
        public ushort bitDepth;
        public ushort interlace;
        public ushort unk5;
        public ushort mipmapStatus;
        public ushort unk7;
        public ushort unk8;

        public uint mipmapHeader1Offset;
        public uint mipmapHeader2Offset;
        public uint unkx18;
        public uint unkx1C;

        public uint indexesOffset;
        public uint paletteOffset;
        public uint unkx28;
        public uint unkx2C;

        public TplImageHeader mipmapHeader1 = null;
        public TplImageHeader mipmapHeader2 = null;

        public string texturePath = null;

    }
}
