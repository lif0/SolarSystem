using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpGL;
using SharpGL.WinForms;
using SharpGL.SceneGraph;
using SharpGL.Enumerations;
namespace WindowsFormsApplication4
{
    public partial class Form1 : Form
    {
        OpenGL gl;
        float year, day = 0;
        double rt = 180.0;
        int view = 0;
        bool stars = true;
        bool orbits = true;
        #region Скорости вращения планет
        const float mercury = 4.0923f;//    360°/Сидерический период обращения - 87,969 дня
        const float venus = 1.6021f;//      360°/Сидерический период обращения - 224,698 дня
        const float eath = 0.98f; //        360°/Сидерический период обращения - 365 дня
        const float moon = 13.1868f;//      360°/Сидерический период обращения - 27.3 дня
        const float mars = 0.5240f;//       360°/Сидерический период обращения - 686,98 дня
        const float jupiter = 0.0830f;//    360°/Сидерический период обращения - 4332,589 дня
        const float saturn = 0.9521f;//     360°/Сидерический период обращения - 378,09 дня
        const float uranus = 0.0117f;//     360°/Сидерический период обращения - 30 685,4 дня
        const float neptune = 0.0059f;//    360°/Сидерический период обращения - 60 190,03 дня
        #endregion

        SharpGL.SceneGraph.Quadrics.Sphere sp;

        public Form1()
        {
            InitializeComponent();
            Random k = new Random();
            for (int i = 0; i < n; i++)
            {
                x[i] = k.Next(-100, 100);
                y[i] = k.Next(-100, 100);
                z[i] = k.Next(-100, 100);
                r[i] = k.Next() / 2 + 0.5;
                g[i] = k.Next() / 2 + 0.5;
                b[i] = k.Next() / 2 + 0.5;
            }

            openGLControl1.MouseWheel += OpenGLControl1_MouseWheel;
            openGLControl1.OpenGLDraw += openGLControl1_OpenGLDraw;
        }

        private void OpenGLControl1_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta > 0)
                view+=e.Delta/120;
            else
                view+= e.Delta/120;
        }

        private void drawSphere2(double r, int nl, int nb)
        {
            double db = Math.PI / nb;
            double dl = 2 * Math.PI / nl;
            gl.PolygonMode(FaceMode.FrontAndBack, PolygonMode.Filled);
            gl.Begin(OpenGL.GL_QUAD_STRIP);
            for (double b = 0; b < nb; b += db)
            {
                double px = r * Math.Cos(b);
                double py = r * Math.Cos(b);
                double z0 = r * Math.Sin(b);
                double px3 = r * Math.Cos(b + db);
                double py3 = r * Math.Cos(b + db);
                double z3 = r * Math.Sin(b + db);

                for (double l = 0; l < nl; l += dl)
                {
                    double x0 = px * Math.Sin(l);
                    double y0 = py * Math.Cos(l);
                    double x3 = px3 * Math.Sin(l);
                    double y3 = py3 * Math.Cos(l);
                    gl.Vertex(x0, y0, z0);
                    gl.Vertex(x3, y3, z3);
                }            
            }
            gl.End(); 
        }
        private void drawOrbit(float radius)
        {
            gl.PushMatrix();
            //Рисование орбиты
            gl.Color(0.5f, 0.5f, 0.5f, 1.0f);
            int np = 50;
            double dx = 2 * Math.PI / np;
            gl.Begin(OpenGL.GL_LINE_LOOP);
            for (double x = 0; x < np; x++)
                gl.Vertex(radius * Math.Cos(dx * x), 0.0, radius * Math.Sin(dx * x));
            gl.End();

            gl.PopMatrix();
        }

        private void openGLControl1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.D: day += 10; break;
                case Keys.S: day -= 10; break;
                case Keys.Y: year = (year + 5) % 360; break;
                case Keys.T: year = (year - 5) % 360; break;
            }
        }

        static int n = 10000;
        int[] x = new int[n];
        int[] y = new int[n];
        int[] z = new int[n];
        double[] r = new double[n];
        double[] g = new double[n];
        double[] b = new double[n];



        private void openGLControl1_OpenGLDraw(object sender, RenderEventArgs args)
        {

            
            gl.Enable(OpenGL.GL_DEPTH_TEST);
            gl.DepthFunc(OpenGL.GL_LEQUAL);
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            //gl.MatrixMode(OpenGL.GL_MODELVIEW);

            //Освещение
            gl.Enable(OpenGL.GL_LIGHTING);
            gl.Enable(OpenGL.GL_COLOR_MATERIAL);
            gl.ColorMaterial(OpenGL.GL_FRONT, OpenGL.GL_AMBIENT);
            float[] pos = { 0f, 0f, 0f,1f };
            float[] light = { 1f, 1f, 1f, 1f };
            float[] ambient = { 0.2f, 0.2f, 0.2f, 1f };
            float[] specular = { 0.0f, 0.0f, 0.0f, 0.0f };
            float spot_cutoff = 360f;
            //float[] spot_derection = { 1.0f, 1.0f, 0.0f, -1.0f };

            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_AMBIENT, ambient);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_POSITION, pos);
            //gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPOT_CUTOFF, spot_cutoff);
            ////gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPOT_DIRECTION, spot_derection);
            ////gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_DIFFUSE, light);
            //gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPECULAR, specular);
            

            float[] MaterialSpecular = { 0.0f, 0.0f, 0.0f, 1.0f };
            float[] MaterialAmbient = { .4f, 0.4f, 0.4f, 1.0f };
            //gl.LightModel(OpenGL.GL_LIGHT_MODEL_AMBIENT, MaterialAmbient);
            //gl.LightModel(OpenGL.GL_SPECULAR, MaterialSpecular);
            gl.Enable(OpenGL.GL_LIGHT0);


            //initlighting();
            //float[] ambient = { 1f, 1f, 1f, 1.0f };
            //float[] black = { 0f, 0f, 0f, 1.0f };
            //float[] diffuse = { 1f, 1f, 1f, 1f };
            //float[] position = { 0f, 0f, 0f, 1.0f};
            //gl.Enable(OpenGL.GL_LIGHTING);
            //gl.LightModel(OpenGL.GL_LIGHT_MODEL_AMBIENT, black);
            //gl.Enable(OpenGL.GL_LIGHT0);
            //gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_AMBIENT, ambient);
            //gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_DIFFUSE, diffuse);
            //gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_POSITION, position);

            //gl.Enable(OpenGL.GL_COLOR_MATERIAL);
            //gl.ColorMaterial(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_AMBIENT);
            //конец освещения

            gl.LoadIdentity();
            gl.ClearColor(0.0f, 0.0f, 0.0f, 0f);

            

            if (stars)
            {
                #region Рисование звезд
                gl.PointSize(2);
                gl.Enable(OpenGL.GL_POINT_SMOOTH);
                gl.Begin(BeginMode.Points);
                for (int i = 0; i < n; i++)
                {
                    gl.Color(r[i], g[i], b[i]);
                    gl.Vertex(x[i], y[i], z[i]);
                }
                gl.End();
                #endregion
            }



            gl.LookAt(0, view, 15, 0, 0, 0, 0, 1, 0);
            gl.Rotate(15, 1, 0, 0);
   
            gl.Color(0.5f, 0.5f, 0.5f, 0.0f);
            if (orbits)
            {
                #region Рисуем орбиты планетам
                gl.Enable(OpenGL.GL_SMOOTH);
                gl.Enable(OpenGL.GL_LINE_STIPPLE);
                gl.LineStipple(1, 0x00FF);
                drawOrbit(2);//Орбита Меркурия
                drawOrbit(3);//Орбита Венеры
                drawOrbit(4);//Орбита Земли
                drawOrbit(5);//Орбита Марса
                drawOrbit(6);//Орбита Юпитера
                drawOrbit(7);//Орбита Сатурна
                drawOrbit(8);//Орбита Урана
                drawOrbit(9);//Орбита Нептуна
                gl.Disable(OpenGL.GL_SMOOTH);
                gl.Disable(OpenGL.GL_LINE_STIPPLE);
                #endregion
            }



            //gl.Normal(0f, -50f, 0f);
            gl.Color(1.0f, 1.0f, 0.0f);
            drawSphere(1.5f, 25, 25);//Солнце            

            //sp.Material.Texture.Create(gl, @"C:\Users\Abraham\Desktop\Задания OpenGL\IMG_8319.JPG");

            #region Рисуем планеты солнечной системы
            gl.PushMatrix();
            #region Меркурий
            gl.PushMatrix();
            gl.Rotate(mercury * day, 0, 1, 0);//Вокруг солнца
            gl.Translate(2, 0, 0);
            gl.Color(0.5f, 0.5f, 0.5f);
            //gl.Rotate(rt, 0, 1, 0);//Вокруг себя
            drawSphere(0.12f, 25, 25);
            gl.PopMatrix();
            #endregion
            #region Венера
            gl.PushMatrix();
            gl.Rotate(venus * day, 0, 1, 0);//Вокруг солнца
            gl.Translate(3, 0, 0);
            gl.Color(0.7f, 0.7f, 0.0f, 0.01f);
            //gl.Rotate(rt, 0, 1, 0);//Вокруг себя
            drawSphere(0.12f, 25, 25);
            gl.PopMatrix();
            #endregion
            #region Земля
            gl.PushMatrix();
            gl.Rotate(eath * day, 0, 1, 0);//Вокруг солнца
            gl.Translate(4, 0, 0);
            gl.Color(0.0f, 1.0f, 1.0f);
            //gl.Rotate(rt, 0, 1, 0);//Вокруг себя
            drawSphere(0.25f, 25, 25);
            #region Луна
            gl.PushMatrix();
            gl.Rotate(moon * day, 0, 1, 0);//Вращение луны Вокруг земли
            gl.Translate(0.5, 0, 0);
            gl.Color(0.5f, 0.5f, 0.5f);
            drawSphere(0.06f, 15, 15);//Луна
            gl.PopMatrix();
            #endregion
            gl.PopMatrix();
            #endregion
            #region Марс
            gl.PushMatrix();
            gl.Rotate(mars * day, 0, 1, 0);//Вокруг солнца
            gl.Translate(5, 0, 0);
            gl.Color(0.7f, 0.1f, 0.1f, 0.01f);
            //gl.Rotate(rt, 0, 1, 0);//Вокруг себя
            drawSphere(0.22f, 25, 25);
            gl.PopMatrix();
            #endregion
            #region Юпитер
            gl.PushMatrix();
            gl.Rotate(jupiter * day, 0, 1, 0);//Вокруг солнца
            gl.Translate(6, 0, 0);
            gl.Color(0.8235f, 0.7373f, 0.6824f, 1f);
            //gl.Rotate(rt, 0, 1, 0);//Вокруг себя
            drawSphere(0.5f, 25, 25);
            gl.PopMatrix();
            #endregion
            #region Сатурн
            gl.PushMatrix();
            gl.Rotate(saturn * day, 0, 1, 0);//Вокруг солнца
            gl.Translate(7, 0, 0);
            gl.Color(1f, 0.66f, 0.117f, 1.0f);
            //gl.Rotate(rt, 0, 1, 0);//Вокруг себя
            gl.Color(0.8824f, 0.7451f, 0.5451f, 1f);
            drawOrbit(0.9f);
            drawSphere(0.5f, 25, 25);
            gl.PopMatrix();
            #endregion
            #region Уран
            gl.PushMatrix();
            gl.Rotate(uranus * day, 0, 1, 0);//Вокруг солнца
            gl.Translate(8, 0, 0);
            gl.Color(0.6196f, 0.7451f, 0.8314f, 1f);
            //gl.Rotate(rt, 0, 1, 0);//Вокруг себя
            drawSphere(0.3f, 25, 25);
            gl.PopMatrix();
            #endregion
            #region Нептун
            gl.PushMatrix();
            gl.Rotate(neptune * day, 0, 1, 0);//Вокруг солнца
            gl.Translate(9, 0, 0);
            gl.Color(0.4275f, 0.5451f, 0.7490f, 1f);
            //gl.Rotate(rt, 0, 1, 0);//Вокруг себя
            drawSphere(0.3f, 30, 30);
            gl.PopMatrix();
            #endregion

            gl.PopMatrix();
            #endregion

            gl.Disable(OpenGL.GL_LIGHT0);
            gl.Disable(OpenGL.GL_LIGHTING);


            //gl.PushMatrix();
            //gl.Rotate(year+50f, 0.0f, 1.0f, 0.0f);
            //gl.Translate(0.5, 0, 0);
            //gl.Rotate(day, 0, 1, 0);

            //gl.Color(0.5f, 0.5f, 0.5f);
            //sphere(0.1f, 20, 20);


            //gl.PopMatrix();
            gl.Disable(OpenGL.GL_DEPTH_TEST);



            try
            {
                ++day;
            }
            catch (Exception)
            {
                day = 0;
            }//++day

            //rt++;

            
        }
        private void drawSphere(double r, int nl, int nb)
        {
            gl.Enable(OpenGL.GL_RESCALE_NORMAL_EXT);
            IntPtr sph = gl.NewQuadric();
            gl.QuadricDrawStyle(sph, OpenGL.GLU_FILL);
            gl.QuadricNormals(sph, OpenGL.GL_SMOOTH);
            gl.Sphere(sph, r, nl, nb);
        }

        private void openGLControl1_OpenGLDraw2(object sender, RenderEventArgs args)
        {
            IntPtr quad = gl.NewQuadric();

            gl.ClearColor(0f, 0f, 0f, 10f);
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            gl.LoadIdentity();

            gl.LookAt(0, 0, 5, 0, 0, 0, 0, 1, 0);


            gl.Enable(OpenGL.GL_LIGHTING);
            float[] pos = { -2f, 0f, 0f, 1f };
            float[] light = { 1f, 1f, 1f, 1f };
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_POSITION, pos);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_DIFFUSE, light);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPECULAR, light);
            gl.Enable(OpenGL.GL_LIGHT0);


            gl.PushMatrix();
            //gl.Enable(OpenGL.GL_RESCALE_NORMAL_EXT);
            gl.Color(1f, 1f, 0f, 1f);
            IntPtr sph = gl.NewQuadric();
            gl.QuadricDrawStyle(sph, OpenGL.GLU_LINE);
            gl.QuadricNormals(sph, OpenGL.GL_SMOOTH);
            gl.Sphere(sph, 1, 25, 25);

            gl.PopMatrix();
            
            //sp.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
        }

        private void openGLControl1_OpenGLInitialized(object sender, EventArgs e)
        {
            
            gl = openGLControl1.OpenGL;
            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.LoadIdentity();
            gl.Perspective(60.0f, openGLControl1.Width / openGLControl1.Height, 1.0f, 100.0f);
        }


        //Фоновое рассеяное освещение
        private void initlighting()
        {
            gl.PushMatrix();
            gl.LoadIdentity();
            float[] l_diffuse = { 0.4f, 0.7f, 0.2f };
            float[] l_pos = { -1.5f, -1.5f, 1.5f, 0 };
            float[] s_dir = { 1, 1, 0, -1 };
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_DIFFUSE, l_diffuse);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_POSITION, l_pos);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_CONSTANT_ATTENUATION, 0.0f);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_LINEAR_ATTENUATION, 0.2f);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_QUADRATIC_ATTENUATION, 0.4f);
            gl.PopMatrix();


            //float[] mat_specular = { 1.0f, 1.0f, 1.0f, 1.0f };
            //float[] mat_shininess = { 50.0f };
            //float[] light_pos = { 0.0f,0.0f,0.0f, 1.0f };
            //float[] white_light = { 1.0f, 1.0f, 1.0f, 1.0f };
            //float[] lmodel_ambient = { 1.0f, 1.0f, 1.1f, 1.0f };//Цвет фонового излучения источника света
            ////gl.GetFloat(OpenGL.GL_FLOAT,)
            //gl.ShadeModel(OpenGL.GL_SMOOTH);
            //gl.Material(OpenGL.GL_FRONT, OpenGL.GL_SPECULAR, mat_specular);
            //gl.Material(OpenGL.GL_FRONT, OpenGL.GL_SHININESS, mat_shininess);
            //gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_POSITION, light_pos);
            //gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPECULAR, white_light);
            //gl.LightModel(OpenGL.GL_LIGHT_MODEL_AMBIENT, lmodel_ambient);

            //gl.Enable(OpenGL.GL_LIGHTING);
            //gl.Enable(OpenGL.GL_LIGHT0);
            //gl.Enable(OpenGL.GL_DEPTH_TEST);
        }
    }
}