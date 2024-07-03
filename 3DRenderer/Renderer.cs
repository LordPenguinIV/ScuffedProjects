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

            _bitmap = new Bitmap(ClientSize.Width, ClientSize.Height, PixelFormat.Format32bppArgb);

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
            BitmapData bmpData = _bitmap.LockBits(new Rectangle(0, 0, _sizeX, _sizeY), ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);

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
                _cam.Position = new Vector3(_cam.Position.X, _cam.Position.Y, _cam.Position.Z + 1f);
            }
            else if (e.KeyChar == 's')
            {
                _cam.Position = new Vector3(_cam.Position.X, _cam.Position.Y, _cam.Position.Z - 1f);
            }
            else if (e.KeyChar == 'a')
            {
                _cam.Position = new Vector3(_cam.Position.X - 1f, _cam.Position.Y, _cam.Position.Z);
            }
            else if (e.KeyChar == 'd')
            {
                _cam.Position = new Vector3(_cam.Position.X + 1f, _cam.Position.Y, _cam.Position.Z);
            }
            else if (e.KeyChar == 'r')
            {
                _cam.Position = new Vector3(_cam.Position.X, _cam.Position.Y + 1f, _cam.Position.Z);
            }
            else if (e.KeyChar == 'f')
            {
                _cam.Position = new Vector3(_cam.Position.X, _cam.Position.Y - 1f, _cam.Position.Z);
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
                _cam.CurrentRotationAngleZ = (_cam.CurrentRotationAngleZ - 30) % 360;
            }
            else if (e.KeyChar == '9')
            {
                _cam.CurrentRotationAngleZ = (_cam.CurrentRotationAngleZ + 30) % 360;
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
