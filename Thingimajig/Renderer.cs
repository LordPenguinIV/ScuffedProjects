using System.Diagnostics;
using System.Drawing.Imaging;
using System.Numerics;

namespace Thingimajig
{
    public class Renderer : Form
    {
        private Bitmap _bitmap;

        private readonly int _sizeY;
        private readonly int _sizeX;
        private readonly Color _sky;

        private Camera _cam;
        private List<IObject> _scene;

        public Renderer(int size, Color? sky = null)
        {
            _sizeY = size;
            _sizeX = (int)(size * 16f / 9f);
            ClientSize = new Size(_sizeX, _sizeY);

            _bitmap = new Bitmap(ClientSize.Width, ClientSize.Height);
            _sky = sky ?? Color.SkyBlue;

            Text = "Pixel Drawer";
            Paint += Renderer_Paint;
            KeyPress += Renderer_KeyPress;
        }

        public Renderer(int size, List<IObject> scene, Camera cam, Color? sky = null)
        {
            _sizeY = size;
            _sizeX = (int)(size * 16f / 9f);
            ClientSize = new Size(_sizeX, _sizeY);

            _bitmap = new Bitmap(ClientSize.Width, ClientSize.Height);
            _sky = sky ?? Color.SkyBlue;

            _cam = cam;
            _scene = scene;

            Text = "Pixel Drawer";
            Paint += Renderer_Paint;
            KeyPress += Renderer_KeyPress;
        }

        private void Renderer_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawImage(_bitmap, 0, 0);
        }

        public void Draw(Color?[,] img)
        {
            for (int i = 0; i < _sizeY; i++)
            {
                for (int j = 0; j < _sizeX; j++)
                {
                    _bitmap.SetPixel(j, i, img[i, j] ?? _sky);
                }
            }
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
            else
            {
                Debug.WriteLine(e.KeyChar);
            }

            _cam.RecalculationRotationMatrix();
            Draw(_cam.GetViewingPlane(_scene));
            Refresh();
        }

        public void Run()
        {
            //_bitmap.Save("img.png", ImageFormat.Png);
            Application.EnableVisualStyles();
            Application.Run(this);
        }
    }

}
