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
        }

        private void button1_Click(object sender, EventArgs e)
        {
            tsd.DrawingHandler dh = new tsd.DrawingHandler();

            ui.DrawingObjectSelector dos = dh.GetDrawingObjectSelector();
            //tsd.DrawingObjectEnumerator doe = dos.GetSelected();

            tsd.ViewBase vv = null;
            tsd.DrawingObject dobj = null;

            ui.Picker picker = dh.GetPicker();

            picker.PickObject("", out dobj, out vv);


            List<tsd.ReinforcementGroup> groups = new List<tsd.ReinforcementGroup>();


            groups.Add(dobj as tsd.ReinforcementGroup);

           /* while(doe.MoveNext())
            {
                //
                if (doe.Current is tsd.ReinforcementGroup)
                {
                    groups.Add(doe.Current as tsd.ReinforcementGroup);
                }
            }*/

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

                /*List<Point> points = new List<Point>();
                foreach(var ps in rg.Polygons)
                {
                    foreach(var pps in ((Polygon)ps).Points)
                    {
                        points.Add(m.Transform(pps as Point));
                    }
                }*/

               /* string axis = points[0].X - points[1].X != 0 ? "x" : points[1].Y - points[0].Y != 0 ? "y" : "z";

                //tsd.StraightDimensionSet sds = new tsd.StraightDimensionSetHandler().CreateDimensionSet(;
                tsd.PointList pls = new tsd.PointList();
                Vector v1 = new Vector(1, 0, 0);
                Vector v2 = new Vector(0, 1, 0);
                Vector v3 = new Vector(0, 0, 1);

                Point begin = new Point(min.X, min.Y, min.Z);
                pls.Add(m.Transform(begin));
                foreach (double d in dist)
                {
                    if (axis == "x")
                    {
                        begin.X += d;
                    }
                    else if (axis == "y")
                    {
                        begin.Y += d;
                    }
                    else
                    {
                        begin.Z += d;
                    }
                    pls.Add(m.Transform(begin));
                }

                try
                {
                    new tsd.StraightDimensionSetHandler().CreateDimensionSet(vw, pls, v1, 100);
                }
                catch { }
                try
                {
                    new tsd.StraightDimensionSetHandler().CreateDimensionSet(vw, pls, v2, 100);
                }
                catch { }
                try
                {
                    new tsd.StraightDimensionSetHandler().CreateDimensionSet(vw, pls, v3, 100);
                }
                catch { }*/

            }
        }
    }
}
