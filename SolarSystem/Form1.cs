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
using SharpGL.SceneGraph.Assets;

namespace WindowsFormsApplication4
{
    public partial class Form1 : Form
    {
        OpenGL gl;
        float day;//Количество пройденных дней               (+D -S)
        const float hour = 0.04167f;//Движение в час 1/24
        int view = 0;//Угол обзора

        bool stars = true;//Отображать звезды?               (+-Z)
        bool orbits = true;//Отображать орибиту?             (+-O)         
        bool fog = false;//Включить туман?                   (+-G)
        bool rotate_cam = false;//Вращать камеру по кругу?   (+-R)
        bool pause = false;//Пауза                           (+-P)

        float rotate_cam_Angle = 0.0f;
        #region Сидерический период обращения
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

        #region Период вращения
        const float sunRP = 0.5910f;//        360°/Период вращения - 25ᵈ 9ʰ 7ᵐ((25*24+9)*60+7/60) часа
        //const float mercuryRP = 0f;//       360°/Период вращения -
        //const float venusRP = 0f;//         360°/Период вращения -
        const float eathRP = 15.0417f; //     360°/Период вращения - 23ʰ 56ᵐ 4.0910(23*60 + 56) часа 
        const float moonRP = 13.33f;//        360°/Период вращения - 27.3 дня
        const float marsRP = 14.6242f;//      360°/Период вращения - 1ᵈ 0ʰ 37ᵐ (24*60 + 37) часа
        const float jupiterRP = 79.7048f;//   360°/Период вращения - 9ʰ 55ᵐ 29.37(9*60+55) часа
        const float saturnRP = 77.4194f;//    360°/Период вращения - 0ᵈ 10ʰ 39ᵐ
        //const float uranusRP = 0f;//        360°/Период вращения - 30 685,4 дня
        //const float neptuneRP = 0f;//       360°/Период вращения - 60 190,03 дня
        #endregion

        IntPtr Qsun, Qmercury, Qvenus, Qeath, Qmoon, Qmars, Qjupiter, Qsaturn, Quranus, Qneptune; 
        Texture Tsun, Tmercury, Tvenus, Teath, Tmoon, Tmars, Tjupiter, Tsaturn, Turanus, Tneptune, Tstars;

        public Form1()
        {
            InitializeComponent();
            Random k = new Random();
            for (int i = 0; i < n; i++)
            {
                x[i] = k.Next(-100, 100);
                y[i] = k.Next(-100, 100);
                z[i] = k.Next(-100, 100);
                colorGrey[i] = k.NextDouble() / 2 + 0.5;
            }

            openGLControl1.MouseWheel += OpenGLControl1_MouseWheel;
            openGLControl1.OpenGLDraw += openGLControl1_OpenGLDraw;

            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;

            openGLControl1_KeyDown(null, new KeyEventArgs(Keys.F1));
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Random k = new Random();
            for (int i = 0; i < n; i++)
                colorGrey[i] = k.NextDouble()+0.4f;
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
            //Рисование орбиты
            int np = 50;
            double dx = 2 * Math.PI / np;        
            gl.Begin(OpenGL.GL_LINE_LOOP);
            for (double x = 0; x < np; x++)
                gl.Vertex(radius * Math.Cos(dx * x), 0.0, radius * Math.Sin(dx * x));
            gl.End();
        }



        static int n = 10000;
        int[] x = new int[n];
        int[] y = new int[n];
        int[] z = new int[n];
        double[] colorGrey = new double[n];
        float[][] constellation_LittleBear =
        {
            new float[] {0f, 0f, 0f },
            new float[] {1f, -0.7f, 0f},
            new float[] {2f, -1f, 0f},
            new float[] {3f, -0.8f, 0f},
            new float[] {4f, -0.5f, 0f},
            new float[] {4.3f, -1f, 0f},
            new float[] {3.4f, -1.25f, 0f},
            new float[] {3f, -0.8f, 0f}
        };
        private void openGLControl1_OpenGLInitialized(object sender, EventArgs e)
        {

            gl = openGLControl1.OpenGL;
            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.LoadIdentity();
            gl.Perspective(60.0f, openGLControl1.Width / openGLControl1.Height, 1.0f, 100.0f);

            initTextures();//Создаем текстуры
            initPlanet();//Создаем модели планет
        }
           
        private void openGLControl1_OpenGLDraw(object sender, RenderEventArgs args)
        {
            gl.Enable(OpenGL.GL_DEPTH_TEST);
            gl.DepthFunc(OpenGL.GL_LEQUAL);
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.Enable(OpenGL.GL_TEXTURE_2D);

            #region Вращение камеры вокруг солнца
            if (rotate_cam)
            {
                float TimeDelta = 0.15f;
                gl.Rotate(rotate_cam_Angle * TimeDelta, 0, 1, 0);//Камеры

                rotate_cam_Angle += 1;//Добовляем 1 градус
            }
            else
                rotate_cam_Angle = 0;//Если вращение отключеноь,то вовзращаемся в начальную позицию
            #endregion

            #region Туман
            if (fog)
            {
                gl.PushMatrix();
                float[] fogColor = new float[4] { 0.25f, 0.25f, 0.25f, 1.0f }; // Цвет тумана 
                gl.Enable(OpenGL.GL_FOG); // Включает туман (GL_FOG) 
                gl.Fog(OpenGL.GL_FOG_MODE, OpenGL.GL_EXP);// Выбираем тип тумана GL_EXP,GL_EXP2,GL_LINEAR
                gl.Fog(OpenGL.GL_FOG_COLOR, fogColor); // Устанавливаем цвет тумана 
                gl.Fog(OpenGL.GL_FOG_DENSITY, 0.02f); // Насколько густым будет туман 
                gl.Hint(OpenGL.GL_FOG_HINT, OpenGL.GL_DONT_CARE); // Вспомогательная установка тумана 
                gl.Fog(OpenGL.GL_FOG_START, -10.0f); // Глубина, с которой начинается туман
                gl.Fog(OpenGL.GL_FOG_END, 1.0f); // Глубина, где туман заканчивается. 

                gl.PopMatrix();
            }
            else
                gl.Disable(OpenGL.GL_FOG);
            #endregion

            #region Рисуем солнце
            Tsun.Bind(gl);//устанавливаем текстуру солнца

            gl.PushMatrix();
            gl.Rotate(day * sunRP, 0, 1, 0);//вращение солнца
            gl.Rotate(-270, 0, 0);
            gl.Color(1f, 1f, 1f);//Цвет которым оно светит
            gl.Sphere(Qsun, 1.5f, 40, 40);//Прорисовываем квадрик
            gl.PopMatrix();
            #endregion


            #region Звезды и создвездия
            if (stars)
            {
                Tstars.Bind(gl);//Текестура звезд
                #region Рисование звезд

                gl.PointSize(1.5f);
                gl.Enable(OpenGL.GL_POINT_SMOOTH);
                gl.Begin(BeginMode.Points);
                for (int i = 0; i < n; i++)
                {
                    //gl.Color(5f, 5f, 5f);
                    gl.Color(colorGrey[i], colorGrey[i], colorGrey[i]);
                    //gl.Color(cl_Grey, cl_Grey, cl_Grey);
                    gl.Vertex(x[i], y[i], z[i]);
                }
                gl.End();
                gl.Disable(OpenGL.GL_POINT_SMOOTH);
                #endregion
                #region Рисование созвездия       
                gl.PointSize(3);
                gl.Enable(OpenGL.GL_POINT_SMOOTH);
                gl.Begin(OpenGL.GL_POINTS);
                foreach (float[] vertex in constellation_LittleBear)
                {
                    gl.Vertex(vertex[0] - 10, vertex[1] + 3, -30);
                }
                gl.End();
                gl.Color(1.0f, 1.0f, 1.0f);
                gl.Begin(OpenGL.GL_LINE_STRIP);
                foreach (float[] vertex in constellation_LittleBear)
                {
                    gl.Vertex(vertex[0] - 10, vertex[1] + 3, -30);
                }
                gl.End();
                #endregion
            }
            #endregion

            #region Рисуем орбиты планетам
            gl.Color(0.2f, 0.2f, 0.2f, 1f);//Цвет орит
            if (orbits)
            {
                gl.Enable(OpenGL.GL_SMOOTH);
                gl.Enable(OpenGL.GL_LINE_STIPPLE);
                gl.LineStipple(1, 0x00FF);
                drawOrbit(2);//Орбита Меркурия
                drawOrbit(3);//Орбита Венеры
                drawOrbit(4);//Орбита Земли
                drawOrbit(5);//Орбита Марса
                drawOrbit(7);//Орбита Юпитера
                drawOrbit(9);//Орбита Сатурна
                drawOrbit(11);//Орбита Урана
                drawOrbit(13);//Орбита Нептуна
                gl.Disable(OpenGL.GL_SMOOTH);
                gl.Disable(OpenGL.GL_LINE_STIPPLE);
            }
            #endregion

            initlighting();//Источник света(Солнце)
            gl.LoadIdentity();

            #region Вкл/Выкл Туман
            if (fog)//Если туман включен
                gl.ClearColor(0.1f, 0.1f, 0.1f, 0f);//Очищаем экран.Цвет фона.
            else
                gl.ClearColor(0.0f, 0.0f, 0.0f, 0f);//Очищаем экран.Цвет фона
            #endregion
            gl.LookAt(0, view, 15, 0, 0, 0, 0, 1, 0);
            gl.Rotate(15, 1, 0, 0);

            #region Рисуем планеты солнечной системы
            gl.PushMatrix();

            #region Меркурий
            gl.PushMatrix();
            gl.Rotate(mercury * day, 0, 1, 0);//Вокруг солнца
            gl.Translate(2, 0, 0);
            gl.Color(0.5f, 0.5f, 0.5f);
            //gl.Rotate(rt, 0, 1, 0);//Вокруг себя
            //drawSphere(0.12f, 25, 25);
            Tmercury.Bind(gl);
            gl.Sphere(Qmercury, 0.12f, 25, 25);
            gl.PopMatrix();
            #endregion
            #region Венера
            gl.PushMatrix();
            gl.Rotate(venus * day, 0, 1, 0);//Вокруг солнца
            gl.Translate(3, 0, 0);
            gl.Color(0.7f, 0.7f, 0.0f, 0.01f);
            //gl.Rotate(rt, 0, 1, 0);//Вокруг себя
            //drawSphere(0.12f, 25, 25);
            Tvenus.Bind(gl);
            gl.Sphere(Qvenus, 0.12f, 25, 25);
            gl.PopMatrix();
            #endregion
            #region Земля
            gl.PushMatrix();
            gl.Rotate(eath * day, 0, 1, 0);//Вокруг солнца
            gl.Translate(4, 0, 0);
            gl.Color(0.0f, 1.0f, 1.0f);
            Teath.Bind(gl);

            gl.Rotate(day * eathRP, 0, 1, 0);//вращение вокруг себя
            gl.PushMatrix();
            gl.Rotate(-270, 0, 0);//Развернуть планету правильно к солнцу
            gl.Sphere(Qeath, 0.25f, 25, 25);
            gl.PopMatrix();
            #region Луна
            gl.PushMatrix();
            gl.Rotate((moon + moonRP) * day, 0, 1, 0);//Вращение луны Вокруг земли
            gl.Translate(0.5, 0, 0);
            gl.Color(0.5f, 0.5f, 0.5f);
            //drawSphere(0.06f, 15, 15);//Луна
            Tmoon.Bind(gl);
            gl.Sphere(Qmoon, 0.06f, 15, 15);
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
            //drawSphere(0.22f, 25, 25);
            Tmars.Bind(gl);

            gl.Rotate(day * marsRP, 0, 1, 0);//вращение вокруг себя
            gl.PushMatrix();
            gl.Rotate(-270, 0, 0);//Развернуть планету правильно к солнцу
            gl.Sphere(Qmars, 0.22f, 25, 25);
            gl.PopMatrix();
            gl.PopMatrix();
            #endregion
            #region Юпитер
            gl.PushMatrix();
            gl.Rotate(jupiter * day, 0, 1, 0);//Вокруг солнца
            gl.Translate(7, 0, 0);
            gl.Color(0.8235f, 0.7373f, 0.6824f, 1f);
            //gl.Rotate(rt, 0, 1, 0);//Вокруг себя
            //drawSphere(0.5f, 25, 25);
            Tjupiter.Bind(gl);


            gl.Rotate(day * jupiterRP, 0, 1, 0);//вращение вокруг себя
            gl.PushMatrix();
            gl.Rotate(-270, 0, 0);//Вокруг себя
            gl.Sphere(Qjupiter, 0.5f, 25, 25);
            gl.PopMatrix();

            float io_or = 1.77f;
            float europe_or = 3.55f;
            float ganimed_or = 7.15f;
            float calisto_or = 16.69f;
            //Орибиты юпитера
            #region Спутник ИО

            gl.PushMatrix();
            gl.Rotate((jupiter + io_or) * day, 0, 0.5f, 0);//Вращение луны Вокруг земли

            gl.Translate(0.7, 0, 0);
            gl.Color(0.5f, 0.5f, 0.5f);
            //drawSphere(0.06f, 15, 15);//Луна
            //Tmoon.Bind(gl);

            IntPtr satellite = iniQuadric();
            gl.Sphere(satellite, 0.03642f, 10, 10);
            gl.PopMatrix();
            #endregion
            #region Спутник Европа

            gl.PushMatrix();
            gl.Rotate((jupiter + europe_or) * day, 0, 0.5f, 0);//Вращение луны Вокруг земли

            gl.Translate(1, 0, 0);
            gl.Color(0.5f, 0.5f, 0.5f);
            //drawSphere(0.06f, 15, 15);//Луна
            //Tmoon.Bind(gl);

            satellite = iniQuadric();
            gl.Sphere(satellite, 0.03122f, 10, 10);
            gl.PopMatrix();
            #endregion
            #region Спутник Ганимед

            gl.PushMatrix();
            gl.Rotate((jupiter + ganimed_or) * day, 0, 0.5f, 0);//Вращение луны Вокруг земли

            gl.Translate(1.5, 0, 0);
            gl.Color(0.5f, 0.5f, 0.5f);
            //drawSphere(0.06f, 15, 15);//Луна
            //Tmoon.Bind(gl);

            satellite = iniQuadric();
            gl.Sphere(satellite, 0.0526f, 10, 10);
            gl.PopMatrix();
            #endregion
            #region Спутник Каллисто

            gl.PushMatrix();
            gl.Rotate((jupiter + calisto_or) * day, 0, 0.5f, 0);//Вращение луны Вокруг земли

            gl.Translate(1.9, 0, 0);
            gl.Color(0.5f, 0.5f, 0.5f);
            //drawSphere(0.06f, 15, 15);//Луна
            //Tmoon.Bind(gl);

            satellite = iniQuadric();
            gl.Sphere(satellite, 0.0482f, 10, 10);
            gl.PopMatrix();
            #endregion

            gl.PopMatrix();
            #endregion
            #region Сатурн
            gl.PushMatrix();
            gl.Rotate(saturn * day, 0, 1, 0);//Вокруг солнца
            gl.Translate(9, 0, 0);
            gl.Color(1f, 0.66f, 0.117f, 1.0f);
            //gl.Rotate(rt, 0, 1, 0);//Вокруг себя
            gl.Color(0.8824f, 0.7451f, 0.5451f, 1f);
            gl.Rotate(25, 0, 0);
            drawOrbit(0.9f);
            drawOrbit(0.85f);
            drawOrbit(0.75f);
            drawOrbit(0.65f);
            gl.Rotate(-25, 0, 0);
            //drawSphere(0.5f, 25, 25);
            Tsaturn.Bind(gl);

            gl.Rotate(day * saturnRP, 0, 1, 0);//вращение вокруг себя
            gl.PushMatrix();
            gl.Rotate(-270, 0, 0);//Вокруг себя
            gl.Sphere(Qsaturn, 0.5f, 25, 25);
            gl.PopMatrix();
            gl.PopMatrix();
            #endregion
            #region Уран
            gl.PushMatrix();
            gl.Rotate(uranus * day, 0, 1, 0);//Вокруг солнца
            gl.Translate(11, 0, 0);
            gl.Color(0.6196f, 0.7451f, 0.8314f, 1f);
            //gl.Rotate(rt, 0, 1, 0);//Вокруг себя
            //drawSphere(0.3f, 25, 25);
            Turanus.Bind(gl);
            gl.Sphere(Quranus, 0.3f, 25, 25);
            gl.PopMatrix();
            #endregion
            #region Нептун
            gl.PushMatrix();
            gl.Rotate(neptune * day, 0, 1, 0);//Вокруг солнца
            gl.Translate(13, 0, 0);
            gl.Color(0.4275f, 0.5451f, 0.7490f, 1f);
            //gl.Rotate(rt, 0, 1, 0);//Вокруг себя
            //drawSphere(0.3f, 30, 30);
            Tneptune.Bind(gl);
            gl.Sphere(Qneptune, 0.3f, 30, 30);
            gl.PopMatrix();
            #endregion

            gl.PopMatrix();
            #endregion

            gl.Disable(OpenGL.GL_LIGHT0);//
            gl.Disable(OpenGL.GL_LIGHTING);
            gl.Disable(OpenGL.GL_DEPTH_TEST);
            //gl.Disable(OpenGL.GL_TEXTURE_2D);

            if (!pause)
                try
                {
                    ///1 сек = 30 кадров(FrameRate), за 1 кадр прибовляем 6 часов
                    ///1 секунда = 180 часов или 7 дней 5 часов к реальному маштабу
                    day += hour * 6;//Добовляем по 6 часов
                }
                catch (Exception)
                {
                    day = 0;
                }
        }

        private string ConvertText(string str)
        {
            string result = "";

            byte[] asci = Encoding.Default.GetBytes(str);

            foreach (byte c in asci)
                result += Convert.ToChar(c).ToString();

            return result;
        }
        private void initlighting()
        {
            float[] materialAmbient = { 0.05f, 0.05f, 0.05f, 1.0f };
            float[] materialDiffuse = { 1f, 1f, 1f, 1.0f };
            float[] materialShininess = { 10.0f };
            float[] lightPosition = { 0f, 0f, 0f, 1.0f };
            float[] lightAmbient = { 0.75f, 0.75f, 0.75f, 1.0f };

            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_AMBIENT, lightAmbient);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_POSITION, lightPosition);
            gl.Material(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_SHININESS, materialShininess);
            gl.Material(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_DIFFUSE, materialDiffuse);
            gl.Material(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_AMBIENT, materialAmbient);

            gl.Enable(OpenGL.GL_LIGHTING);
            gl.Enable(OpenGL.GL_LIGHT0);
        }//Инициализия освещения
        private void initTextures()
        {
            Tstars =  new Texture();
            Tstars.Create(gl, new Bitmap("texture\\starsColor.bmp"));
            Bitmap bmp;
            #region Текстура Солнца
            Tsun = new Texture();
            bmp = new Bitmap(@"texture/sunmap.jpg");
            bmp.RotateFlip(RotateFlipType.RotateNoneFlipNone);
            Tsun.Create(gl, bmp);
            #endregion
            #region Текстура Меркурия
            Tmercury = new Texture();
            bmp = new Bitmap(@"texture/mercurymap.jpg");
            bmp.RotateFlip(RotateFlipType.RotateNoneFlipNone);
            Tmercury.Create(gl, bmp);
            #endregion
            #region Текстура Венеры
            Tvenus = new Texture();
            bmp = new Bitmap(@"texture/venusmap.jpg");
            bmp.RotateFlip(RotateFlipType.RotateNoneFlipNone);
            Tvenus.Create(gl, bmp);
            #endregion
            #region Текстура Земли
            Teath = new Texture();
            bmp = new Bitmap(@"texture/earthmap.jpg");
            bmp.RotateFlip(RotateFlipType.RotateNoneFlipNone);
            Teath.Create(gl, bmp);
            #endregion
            #region Текстура Луты
            Tmoon = new Texture();
            bmp = new Bitmap(@"texture/moonmap.jpg");
            bmp.RotateFlip(RotateFlipType.RotateNoneFlipNone);
            Tmoon.Create(gl, bmp);
            #endregion
            #region Текстура Марса
            Tmars = new Texture();
            bmp = new Bitmap(@"texture/marsmap.jpg");
            bmp.RotateFlip(RotateFlipType.RotateNoneFlipNone);
            Tmars.Create(gl, bmp);
            #endregion
            #region Текстура Юпитера
            Tjupiter = new Texture();
            bmp = new Bitmap(@"texture/jupitermap.jpg");
            bmp.RotateFlip(RotateFlipType.RotateNoneFlipNone);
            Tjupiter.Create(gl, bmp);
            #endregion
            #region Текстура Сатурна
            Tsaturn = new Texture();
            bmp = new Bitmap(@"texture/saturn_surface.jpg");
            bmp.RotateFlip(RotateFlipType.RotateNoneFlipNone);
            Tsaturn.Create(gl, bmp);
            #endregion
            #region Текстура Урана
            Turanus = new Texture();
            bmp = new Bitmap(@"texture/uranusmap.jpg");
            bmp.RotateFlip(RotateFlipType.RotateNoneFlipNone);
            Turanus.Create(gl, bmp);
            #endregion
            #region Текстура Нептуна
            Tneptune = new Texture();
            bmp = new Bitmap(@"texture/neptunemap.jpg");
            bmp.RotateFlip(RotateFlipType.RotateNoneFlipNone);
            Tneptune.Create(gl, bmp);
            #endregion
        }//Инициализия текстур
        private void initPlanet()
        {
            Qsun = iniQuadric(OpenGL.GLU_INSIDE);//Инициализируем солнце с нормалями повернутыми внуть
            Qmercury = iniQuadric();//Инициализируем все планеты с нормалями повернутыми наружу
            Qvenus = iniQuadric();
            Qeath = iniQuadric();
            Qmoon = iniQuadric();
            Qmars = iniQuadric();
            Qjupiter = iniQuadric();
            Qsaturn = iniQuadric();
            Quranus = iniQuadric();
            Qneptune = iniQuadric();
        }//Инициализия квадриков планет
        private IntPtr iniQuadric(uint glu_mode_orientation=OpenGL.GLU_OUTSIDE)
        {
            IntPtr planet = gl.NewQuadric();
            gl.QuadricTexture(planet, (int)OpenGL.GL_TRUE);//Активируем текстуру на квадрике
            gl.Enable(OpenGL.GL_RESCALE_NORMAL_EXT);//При однородном масштабирование применяем что нормализовать нормали
            gl.QuadricOrientation(planet, (int)glu_mode_orientation);//Указываем положение нормалей
            gl.QuadricDrawStyle(planet, OpenGL.GLU_FILL);//Тип прорисовки
            gl.QuadricNormals(planet, OpenGL.GL_SMOOTH);//Сглаживание
            return planet;//Возвращаем квадрик
        }//Возвращаем квадрик с установленными настройками

        private void OpenGLControl1_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta > 0)
                view += e.Delta / 120;
            else
                view += e.Delta / 120;
        }
        private void openGLControl1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.D: day += 1; break;
                case Keys.S: day -= 1; break;

                case Keys.P: pause = !pause; break;

                case Keys.G: fog = !fog; break;

                case Keys.R: rotate_cam = !rotate_cam; break;

                case Keys.O: orbits = !orbits; break;
                case Keys.Z: stars = !stars; break;


                case Keys.Escape: Application.Exit(); break;

                case Keys.F2:
                    {
                        if (this.FormBorderStyle == FormBorderStyle.None)
                            this.FormBorderStyle = FormBorderStyle.Sizable;
                        else
                            this.FormBorderStyle = FormBorderStyle.None;
                        break;
                    }
                case Keys.F3:
                    {
                        if (this.WindowState == FormWindowState.Normal)
                            this.WindowState = FormWindowState.Maximized;
                        else
                            this.WindowState = FormWindowState.Normal;
                        break;
                    }
                case Keys.F1:
                    {
                        try
                        {
                            string msg = "F1 - Show info\r\nF2 - Border Style\r\nF3 - Window State\r\n\r\n(D/S) - +/- day\r\n(Z) - Show/Hide stars\r\n(O) - Show/Hide orbits\r\n(G) - Show/Hide fog\r\n(R) - On/Off Rotate cam\r\n(P) - On/Off Pause";
                            MessageBox.Show(msg, "Info");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                        break;
                    }
            }
        }
    }
}