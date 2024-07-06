using System.Diagnostics;
using System.Drawing.Imaging;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Thingimajig
{
    public class Renderer : Form
    {
        private Bitmap _bitmap;

        private readonly int _sizeY;
        private readonly int _sizeX;

        private Camera _cam;

        public Renderer(int size, Camera cam)
        {
            _sizeY = size;
            _sizeX = (int)(size * 16f / 9f);
            ClientSize = new Size(_sizeX, _sizeY);

            _bitmap = new Bitmap(_sizeX, _sizeY, PixelFormat.Format32bppRgb);

            _cam = cam;

            Text = "Pixel Drawer";
            Paint += Renderer_Paint;
            KeyPress += Renderer_KeyPress;
        }

        private void Renderer_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawImage(_bitmap, 0, 0);
        }

        public void Draw(byte[] img)
        {
            BitmapData bmpData = _bitmap.LockBits(new Rectangle(0, 0, _sizeX, _sizeY), ImageLockMode.WriteOnly, _bitmap.PixelFormat);

            IntPtr ptr = bmpData.Scan0;
            Marshal.Copy(img, 0, ptr, img.Length);

            _bitmap.UnlockBits(bmpData);
        }

        private void Renderer_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (_cam == null)
            {
                return;
            }

            if (e.KeyChar == 'w')
            {
                _cam.MoveCamera(new Vector3(0, 0, 1));
            }
            else if (e.KeyChar == 's')
            {
                _cam.MoveCamera(new Vector3(0, 0, -1));
            }
            else if (e.KeyChar == 'a')
            {
                _cam.MoveCamera(new Vector3(-1, 0, 0));
            }
            else if (e.KeyChar == 'd')
            {
                _cam.MoveCamera(new Vector3(1, 0, 0));
            }
            else if (e.KeyChar == 'r')
            {
                _cam.MoveCamera(new Vector3(0, 1, 0));
            }
            else if (e.KeyChar == 'f')
            {
                _cam.MoveCamera(new Vector3(0, -1, 0));
            }
            else if (e.KeyChar == '8')
            {
                _cam.CurrentRotationAngleY = (_cam.CurrentRotationAngleY - 30) % 360;
            }
            else if (e.KeyChar == '5')
            {
                _cam.CurrentRotationAngleY = (_cam.CurrentRotationAngleY + 30) % 360;
            }
            else if (e.KeyChar == '4')
            {
                _cam.CurrentRotationAngleX = (_cam.CurrentRotationAngleX + 30) % 360;
            }
            else if (e.KeyChar == '6')
            {
                _cam.CurrentRotationAngleX = (_cam.CurrentRotationAngleX - 30) % 360;
            }
            else if (e.KeyChar == '7')
            {
                _cam.CurrentRotationAngleZ = (_cam.CurrentRotationAngleZ + 30) % 360;
            }
            else if (e.KeyChar == '9')
            {
                _cam.CurrentRotationAngleZ = (_cam.CurrentRotationAngleZ - 30) % 360;
            }
            else if (e.KeyChar == '+')
            {
                _cam.DoRayTracing = !_cam.DoRayTracing;
            }
            else if (e.KeyChar == ' ')
            {
                _cam.OutputBuffer.Dispose();
                _cam.SceneBuffer.Dispose();
                _cam.Accelerator.Dispose();
                _cam.Context.Dispose();

                Close();
                return;
            }
            else
            {
                Debug.WriteLine(e.KeyChar);
            }

            _cam.RecalculationRotationMatrix();
            var t = new Stopwatch();
            t.Start();
            var v = _cam.GetViewingPlane();
            Draw(v);
            Debug.WriteLine($"Render: {t.ElapsedMilliseconds}ms");
            Refresh();
            t.Stop();
        }

        public void Run()
        {
            //_bitmap.Save("img.png", ImageFormat.Png);
            Application.EnableVisualStyles();
            Application.Run(this);
        }
    }

}
