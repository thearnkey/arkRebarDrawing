using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;

using tsd = Tekla.Structures.Drawing;
using ui = Tekla.Structures.Drawing.UI;
using Tekla.Structures.Model;
using System.Collections;
using TDT = Tekla.Structures.Datatype;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Datatype;
using System.Globalization;

namespace arkRebarDrawing
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            this.TopMost = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            tsd.DrawingHandler dh = new tsd.DrawingHandler();

            ui.DrawingObjectSelector dos = dh.GetDrawingObjectSelector();

            tsd.ViewBase vv = null;
            tsd.DrawingObject dobj = null;

            ui.Picker picker = dh.GetPicker();

            picker.PickObject("", out dobj, out vv);


            List<tsd.ReinforcementGroup> groups = new List<tsd.ReinforcementGroup>();


            groups.Add(dobj as tsd.ReinforcementGroup);

            foreach(var v in groups)
            {

                tsd.View vw = vv as tsd.View;
                Matrix m = MatrixFactory.ToCoordinateSystem(vw.DisplayCoordinateSystem);

                ModelObject mrg = new Model().SelectModelObject(new Tekla.Structures.Identifier(v.ModelIdentifier.ID));

                RebarGroup rg = mrg as RebarGroup;

                string result = "";

                rg.GetReportProperty("CC_EXACT", ref result);

                DistanceList distanceList1 = new DistanceList();
                ArrayList dist = new ArrayList();
                distanceList1 = DistanceList.Parse(result, CultureInfo.InvariantCulture, TDT.Distance.UnitType.Millimeter);
                foreach (TDT.Distance distance in distanceList1)
                    dist.Add(distance.ConvertTo(TDT.Distance.UnitType.Millimeter));
                dist.RemoveAt(0);

                Point min = m.Transform(rg.GetSolid().MinimumPoint);
                Point max = m.Transform(rg.GetSolid().MaximumPoint);

                tsd.Rectangle rectangle = new tsd.Rectangle(vw, min, max);

                rectangle.Insert();
                tsd.Line line = new tsd.Line(vw,
                    (min),
                    (max));
                line.Insert();
                line = new tsd.Line(vw,
                    (new Point(min.X, max.Y, min.Z)),
                    (new Point(max.X, min.Y, min.Z))
                    );
                line.Insert();
            }
        }


        /// <summary>
        /// 0 - X, 1 - Y
        /// </summary>
        /// <param name="axis"></param>
        void RebarDimension(int axis)
        {
            try
            {
                tsd.DrawingHandler dh = new tsd.DrawingHandler();

                ui.DrawingObjectSelector dos = dh.GetDrawingObjectSelector();
                tsd.ViewBase vv = null;
                tsd.DrawingObject dobj = null;

                ui.Picker picker = dh.GetPicker();
                picker.PickObject("", out dobj, out vv);

                tsd.View vw = vv as tsd.View;
                
                WorkPlaneHandler wph = new Model().GetWorkPlaneHandler();
                TransformationPlane tp = wph.GetCurrentTransformationPlane();
                TransformationPlane tp0 = new TransformationPlane(vw.DisplayCoordinateSystem);
                wph.SetCurrentTransformationPlane(tp0);
                try
                {
                    List<tsd.ReinforcementGroup> groups = new List<tsd.ReinforcementGroup>();
                    groups.Add(dobj as tsd.ReinforcementGroup);
                    Matrix m = MatrixFactory.ToCoordinateSystem(vw.DisplayCoordinateSystem);
                    foreach (tsd.ReinforcementGroup rg in groups)
                    {
                        ModelObject mrg = new Model().SelectModelObject(new Tekla.Structures.Identifier(rg.ModelIdentifier.ID));
                        RebarGroup rgm = mrg as RebarGroup;
                        rgm.Select();

                        string result = "";
                        int number = 0;

                        rgm.GetReportProperty("CC", ref result); //175/200
                        rgm.GetReportProperty("NUMBER", ref number);

                        string size = rgm.Size;
                        Point min = (rgm.GetSolid().MinimumPoint);
                        Point max = (rgm.GetSolid().MaximumPoint);
                        tsd.PointList pl = new tsd.PointList();
                        double fst = 0;
                        double step = 0;

                        if (result.Contains("/"))
                        {
                            fst = Convert.ToDouble(result.Split(new char[] { '/' })[0]);
                            step = Convert.ToDouble(result.Split(new char[] { '/' })[1]);
                            number -= 3;
                        }
                        else
                        {
                            step = Convert.ToDouble(result);
                            number -= 1;
                        }

                        //number -= 2;

                        if (axis == 0)
                        {
                            min.X += (Convert.ToDouble(size) * 0.5);
                        }
                        else
                        {
                            min.Y += (Convert.ToDouble(size) * 0.5);
                        }
                        pl.Add(new Point(min));
                        if (axis == 0)
                        {
                            min.X += fst;
                        }
                        else
                        {
                            min.Y += fst;
                        }
                        pl.Add(new Point(min));
                        for (int i = 0; i < number; i++)
                        {
                            if (axis == 0)
                            {
                                min.X += step;
                            }
                            else
                            {
                                min.Y += step;
                            }
                            pl.Add(new Point(min));
                        }
                        if (axis == 0)
                        {
                            min.X += fst;
                        }
                        else
                        {
                            min.Y += fst;
                        }
                        pl.Add(new Point(min));

                        Vector v1 = new Vector(1, 0, 0);
                        Vector v2 = new Vector(0, 1, 0);
                        Vector v3 = new Vector(0, 0, 1);
                        try
                        {
                            new tsd.StraightDimensionSetHandler().CreateDimensionSet(vw, pl, v1, 100);
                        }
                        catch { }
                        try
                        {
                            new tsd.StraightDimensionSetHandler().CreateDimensionSet(vw, pl, v2, 100);
                        }
                        catch { }
                        try
                        {
                            new tsd.StraightDimensionSetHandler().CreateDimensionSet(vw, pl, v3, 100);
                        }
                        catch { }
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Что-то пошло не так");
                }
                finally
                {

                    wph.SetCurrentTransformationPlane(tp);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Выбрать не тот объект");
            }
        }

        private void button3_Click(object sender, EventArgs e) => RebarDimension(1);

        private void button2_Click(object sender, EventArgs e) => RebarDimension(0);

    }
}
