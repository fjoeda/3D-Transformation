using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HelixToolkit;
using HelixToolkit.Wpf;


namespace _3D_Transformation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        Point3D titik_awal_sumbu = new Point3D();
        Point3D titik_akhir_sumbu = new Point3D();
        Point3D posisi_objek = new Point3D();
        Model3DGroup modelGroup = new Model3DGroup();

        Double anglenew, angleold;
        public MainWindow()
        {
            InitializeComponent();
            DrawCartesianAxis();
        }

        private void Draw_Arb_Axis(object sender, RoutedEventArgs e)
        {
            GetSumbuPutar();
            ClearMesh();
            var meshBuilder = new MeshBuilder(false,false);
            meshBuilder.AddPipe(titik_awal_sumbu, titik_akhir_sumbu,0, 3,360);

            var mesh = meshBuilder.ToMesh(true);
            var material = MaterialHelper.CreateMaterial(Colors.Green);
            modelGroup.Children.Add(new GeometryModel3D
            {
                Geometry = mesh,
                Material = material,
                Transform = new TranslateTransform3D(titik_awal_sumbu.ToVector3D())
            });
            this.Model = modelGroup;
            ModelVisual.Content = modelGroup;
            
        }

        private void GetSumbuPutar()
        {
            try
            {
                titik_awal_sumbu = new Point3D(Double.Parse(X_Asal.Text), Double.Parse(Y_Asal.Text), Double.Parse(Z_Asal.Text));
                titik_akhir_sumbu = new Point3D(Double.Parse(X_Tujuan.Text), Double.Parse(Y_Tujuan.Text), Double.Parse(Z_Tujuan.Text));
            }
            catch (Exception)
            {
                MessageBox.Show("Pastikan koordinat terisi semua","Peringatan");
            }
           
        }

        private void GetObjectPosistion()
        {
            try
            {
                posisi_objek = new Point3D(Double.Parse(X_Obj.Text), Double.Parse(Y_Obj.Text), Double.Parse(Z_Obj.Text));
            }
            catch (Exception)
            {
                MessageBox.Show("Pastikan koordinat object terisi semua", "Peringatan");
            }
        }

        private void DrawCartesianAxis()
        {
            var axisBuilder = new MeshBuilder(false, false);
            axisBuilder.AddPipe(new Point3D(-1000, 0, 0), new Point3D(1000, 0, 0), 0, 0.8, 360);
            axisBuilder.AddPipe(new Point3D(0, -1000, 0), new Point3D(0, 1000, 0), 0, 0.8, 360);
            axisBuilder.AddPipe(new Point3D(0, 0, -1000), new Point3D(0, 0, 1000), 0, 0.8, 360);

            var mesh = axisBuilder.ToMesh(true);
            var material = MaterialHelper.CreateMaterial(Colors.Gray);
            modelGroup.Children.Add(new GeometryModel3D
            {
                Geometry = mesh,
                Material = material,
                Transform = new TranslateTransform3D(0,0,0)
            });
            ModelVisual.Content = modelGroup;

        }

        private void ClearMesh()
        {
            modelGroup.Children.Clear();
            DrawCartesianAxis();
        }

        public Model3D Model { get; set; }

        private void Draw_Object(object sender, RoutedEventArgs e)
        {
            GetObjectPosistion();
            if(modelGroup.Children!= null)
            {
                ClearMesh();
                DrawCartesianAxis();
                Draw_Arb_Axis(sender,e);
            }
            
            var objBuilder = new MeshBuilder(false, false);
            objBuilder.AddPyramid(posisi_objek, 15, 20, true);

            var mesh = objBuilder.ToMesh(true);
            var material = MaterialHelper.CreateMaterial(Colors.Yellow);
            modelGroup.Children.Add(new GeometryModel3D
            {
                Geometry = mesh,
                Material = material,
                Transform = new TranslateTransform3D(posisi_objek.ToVector3D())
            });
            ModelVisual.Content = modelGroup;
        }

        private void SetAngle(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            anglenew = AngleSlider.Value;
            Double angle = anglenew - angleold;
            posisi_objek = Vector3D.Multiply(posisi_objek.ToVector3D(), GetRotationMatrixAboutAxes(titik_awal_sumbu, titik_akhir_sumbu, angle)).ToPoint3D();
            if (modelGroup.Children != null)
            {
                ClearMesh();
                DrawCartesianAxis();
                Draw_Arb_Axis(sender,e);
                UpdateObjectDraw();
                Console.WriteLine(anglenew);
                Console.WriteLine(posisi_objek);
            }
            Console.WriteLine(angle);
            angleold = anglenew;
        }

        public Matrix3D GetRotationMatrixAboutAxes(Point3D awal, Point3D akhir, Double angle)
        {
            Vector3D axisDirection = akhir.ToVector3D() - awal.ToVector3D();
            Double L = axisDirection.Length;
            Double V = Math.Sqrt(Math.Pow(axisDirection.Y, 2)+Math.Pow(axisDirection.Z, 2));
            Matrix3D translate2origin = new Matrix3D(
                1, 0, 0, -awal.X, 
                0, 1, 0, -awal.Y, 
                0, 0, 1, -awal.Z, 
                0, 0, 0, 1);
            Matrix3D rotate2xaxis = new Matrix3D(
                1, 0, 0, 0,
                0, axisDirection.Z / V, -axisDirection.Y / V, 0,
                0, axisDirection.Y / V, axisDirection.Z / V, 0,
                0, 0, 0, 1);
            Matrix3D rotate2yaxis = new Matrix3D(
                V / L, 0, -axisDirection.X / L, 0,
                0, 1, 0, 0,
                axisDirection.X / L, 0, V / L, 0,
                0, 0, 0, 1);
           
            
           Matrix3D rotate = new Matrix3D(
                Math.Cos(Math.PI*angle/180), -Math.Sin(Math.PI * angle / 180),0,0,
                Math.Sin(Math.PI * angle / 180), Math.Cos(Math.PI * angle / 180),0,0,
                0,0,1,0,
                0,0,0,1);

            Matrix3D retranslate2origin = new Matrix3D(
                1, 0, 0, awal.X,
                0, 1, 0, awal.Y,
                0, 0, 1, awal.Z,
                0, 0, 0, 1);
            Matrix3D rerotate2xaxis = new Matrix3D(
                1, 0, 0, 0,
                0, axisDirection.Z / V, axisDirection.Y / V, 0,
                0, -axisDirection.Y / V, axisDirection.Z / V, 0,
                0, 0, 0, 1);
            Matrix3D rerotate2yaxis = new Matrix3D(
                V / L, 0, axisDirection.X / L, 0,
                0, 1, 0, 0,
                -axisDirection.X / L, 0, V / L, 0,
                0, 0, 0, 1);
            if (axisDirection.Y == 0 && axisDirection.Z==0)
            {
                rotate2xaxis = Matrix3D.Identity;
                rerotate2xaxis = Matrix3D.Identity;
            }

            Matrix3D finalmatrix = retranslate2origin * rerotate2xaxis * rerotate2yaxis * rotate * rotate2yaxis * rotate2xaxis * translate2origin;
            Console.WriteLine(finalmatrix);
            return finalmatrix;
        }

        private void UpdateObjectDraw()
        {
            
            var objBuilder = new MeshBuilder(false, false);
            objBuilder.AddPyramid(posisi_objek, 15, 20, true);

            var mesh = objBuilder.ToMesh(true);
            var material = MaterialHelper.CreateMaterial(Colors.Yellow);
            modelGroup.Children.Add(new GeometryModel3D
            {
                Geometry = mesh,
                Material = material,
                Transform = new TranslateTransform3D(posisi_objek.ToVector3D())
            });
            ModelVisual.Content = modelGroup;
        }

        
    }
}
