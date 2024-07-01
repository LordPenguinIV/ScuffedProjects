namespace MapGenerator
{
    public class Renderer : Form
    {
        private Bitmap _bitmap { get; set; }

        public int X { get; set; }
        public int Y { get; set; }

        public static Color DeepWater = Color.FromArgb(9, 56, 135);
        public static Color ShallowWater = Color.FromArgb(26, 97, 217);
        public static Color LowLand = Color.FromArgb(20, 87, 38);
        public static Color MidLand = Color.FromArgb(23, 143, 17);
        public static Color HighLand = Color.FromArgb(121, 201, 40);


        public Renderer(int x, int y)
        {
            X = x;
            Y = y;

            ClientSize = new Size(X, Y);

            _bitmap = new Bitmap(ClientSize.Width, ClientSize.Height);

            Text = "Map";
            Paint += Renderer_Paint;
        }

        private void Renderer_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawImage(_bitmap, 0, 0);
        }

        private void Renderer_KeyPress(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawImage(_bitmap, 0, 0);
        }

        public void Draw(int[,] map)
        {
            for (int x = 0; x < X; x++)
            {
                for (int y = 0; y < Y; y++)
                {
                    int val = map[x, y];
                    switch (val)
                    {
                        case < 51:
                            _bitmap.SetPixel(x, y, DeepWater);
                            break;
                        case < 102:
                            _bitmap.SetPixel(x, y, ShallowWater);
                            break;
                        case < 153:
                            _bitmap.SetPixel(x, y, LowLand);
                            break;
                        case < 204:
                            _bitmap.SetPixel(x, y, MidLand);
                            break;
                        case < 255:
                            _bitmap.SetPixel(x, y, HighLand);
                            break;
                    }

                }
            }
        }

        public void Run()
        {
            Application.EnableVisualStyles();
            Application.Run(this);
        }
    }

}
