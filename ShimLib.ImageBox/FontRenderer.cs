using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShimLib {
    public class FontRenderer {
        ImageBuffer[] fontBuffers;
        static FontRenderer() {
            ImageBuffer fontImage = new ImageBuffer(Properties.Resources.tom_thumb_new);
        }
        
        public static void DrawString(IntPtr dispBuf, int bw, int bh, string text, int x, int y) {
            byte[] bytes = Encoding.ASCII.GetBytes(text);
        }
    }
}
