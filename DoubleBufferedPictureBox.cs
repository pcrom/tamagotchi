using System.Windows.Forms;

namespace HanaJotchi
{
    public class DoubleBufferedPictureBox : PictureBox
    {
        public DoubleBufferedPictureBox()
        {
            this.DoubleBuffered = true;
        }
    }
}
