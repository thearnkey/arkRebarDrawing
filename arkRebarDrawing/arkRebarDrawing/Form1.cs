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

        private void CreateDetailView()
        {
            tsd.DrawingHandler dh = new tsd.DrawingHandler();

            ui.DrawingObjectSelector dos = dh.GetDrawingObjectSelector();
            tsd.DrawingObjectEnumerator doe = dos.GetSelected();

            double minX = double.MaxValue;
            double maxX = double.MinValue;

            double minY = double.MaxValue;
            double maxY = double.MinValue;

            double minZ = double.MaxValue;
            double maxZ = double.MinValue;

            tsd.View v = null;

            ArrayList drParts = new ArrayList();

            while (doe.MoveNext())
            {
                if (doe.Current is tsd.ReinforcementGroup)
                {
                    tsd.ReinforcementGroup p = doe.Current as tsd.ReinforcementGroup;
                    v = p.GetView() as tsd.View;
                    drParts.Add(doe.Current);

                    WorkPlaneHandler wph = new Model().GetWorkPlaneHandler();
                    TransformationPlane tp = wph.GetCurrentTransformationPlane();
                    TransformationPlane tp0 = new TransformationPlane(v.DisplayCoordinateSystem);
                    wph.SetCurrentTransformationPlane(tp0);


                    ModelObject mobj = new Model().SelectModelObject(p.ModelIdentifier);

                    Point min = null;
                    Point max = null;

                    if (mobj is RebarGroup)
                    {
                        min = (mobj as RebarGroup).GetSolid().MinimumPoint;
                        max = (mobj as RebarGroup).GetSolid().MaximumPoint;
                    }

                    if (mobj is SingleRebar)
                    {
                        min = (mobj as SingleRebar).GetSolid().MinimumPoint;
                        max = (mobj as SingleRebar).GetSolid().MaximumPoint;
                    }

                    minX = Math.Min(min.X, minX);
                    maxX = Math.Max(min.X, maxX);
                    minX = Math.Min(max.X, minX);
                    maxX = Math.Max(max.X, maxX);

                    minY = Math.Min(min.Y, minY);
                    maxY = Math.Max(min.Y, maxY);
                    minY = Math.Min(max.Y, minY);
                    maxY = Math.Max(max.Y, maxY);

                    minZ = Math.Min(min.Z, minZ);
                    maxZ = Math.Max(min.Z, maxZ);
                    minZ = Math.Min(max.Z, minZ);
                    maxZ = Math.Max(max.Z, maxZ);

                    wph.SetCurrentTransformationPlane(tp);
                }
            }

            double scale = Convert.ToDouble(textBox3.Text);

            Point minPoint = new Point(minX, minY, minZ);
            Point maxPoint = new Point(maxX + scale, maxY + scale, maxZ + scale);
            Point centerPoint = new Point((minX + maxX) * 0.5 - scale, (minY + maxY) * 0.5 - scale, (minZ + maxZ) * 0.5 - scale);
            Point LabelPoint = new Point(maxPoint.X + 2 * scale, maxPoint.Y + 2 * scale, maxPoint.Z + 2 * scale);

            AABB ab = new AABB(minPoint, maxPoint);


            // tsd.ContainerView cv = dh.GetActiveDrawing().GetSheet();
            //tsd.View nv = new tsd.View(cv, v.DisplayCoordinateSystem, v.DisplayCoordinateSystem, drParts);
            //tsd.View nv = new tsd.View(cv, v.DisplayCoordinateSystem, v.DisplayCoordinateSystem, ab);
            // nv.Attributes.Scale = 15;
            // nv.Insert();
            tsd.View dvm = null;
            tsd.DetailMark dm = null;

            tsd.View.ViewAttributes va = new tsd.View.ViewAttributes("Узел");
            va.Scale = 15;
            tsd.TextElement te = new tsd.TextElement("М ");
            te.Font.Name = "GOST 2.304 type A";
            te.Font.Height = 3.5;

            tsd.PropertyElement pe = new tsd.PropertyElement(tsd.PropertyElement.PropertyElementType.DetailViewLabelMarkPropertyElementTypes.Scale());
            pe.Font.Name = "GOST 2.304 type A";
            pe.Font.Height = 3.5;
            tsd.PropertyElement viewname = new tsd.PropertyElement(tsd.PropertyElement.PropertyElementType.ViewLabelMarkPropertyElementTypes.ViewName());
            viewname.Font.Name = "GOST 2.304 type A";
            viewname.Font.Height = 3.5;

            va.TagsAttributes.TagA1.TagContent.Clear();
            va.TagsAttributes.TagA2.TagContent.Clear();
            va.MarkSymbolAttributes.Size = 12;
            va.MarkSymbolAttributes.Shape = tsd.MarkSymbolShape.None;
            va.MarkSymbolAttributes.LineLength = 12;
            va.MarkSymbolAttributes.LineLengthType = tsd.View.LabelLineLengthType.Minimal;

            tsd.TextElement vtext = new tsd.TextElement("Узел ");
            vtext.Font.Name = "GOST 2.304 type A";
            vtext.Font.Height = 3.5;

            va.TagsAttributes.TagA1.TagContent.Add(vtext);
            va.TagsAttributes.TagA1.TagContent.Add(viewname);
            va.TagsAttributes.TagA2.TagContent.Add(te);
            va.TagsAttributes.TagA2.TagContent.Add(pe);

            

            va.TagsAttributes.TagA1.Location = tsd.TagLocation.AboveLine;
            //va.TagsAttributes.TagA1.Offset = new Vector(0, 5, 0);
            va.TagsAttributes.TagA2.Location = tsd.TagLocation.BelowLine;

            tsd.DetailMark.DetailMarkAttributes dma = new tsd.DetailMark.DetailMarkAttributes();

            dma.MarkName = textBox1.Text;
            dma.BoundaryShape = tsd.DetailMark.DetailMarkAttributes.DetailBoundaryShape.Rectangular;

            dma.BoundingLine.Type = tsd.LineTypes.DashDoubleDot;
            dma.BoundingLine.Color = tsd.DrawingColors.Yellow;

            //tsd.TextElement te = new tsd.TextElement(textBox2.Text);
            //te.Font.Name = "Gost 2.304 type A";
            //te.Font.Height = 3.5;

            dma.TagsAttributes.TagA2.TagContent.Clear();
            //dma.TagsAttributes.TagA2.TagContent.Add(te);

            tsd.View.CreateDetailView(v, centerPoint, maxPoint, LabelPoint, new Point(0, 0, 0), va, dma, out dvm, out dm);

           // tsd.DetailMark sm = new tsd.DetailMark(v, CenterPoint, max, labelPoint);

        }

        private void circle(int axis)
        {
            tsd.DrawingHandler dh = new tsd.DrawingHandler();

            ui.DrawingObjectSelector dos = dh.GetDrawingObjectSelector();
            tsd.DrawingObjectEnumerator doe = dos.GetSelected();
            while (doe.MoveNext())
            {
                if (doe.Current is tsd.ReinforcementGroup)
                {
                    tsd.ReinforcementGroup p = doe.Current as tsd.ReinforcementGroup;
                    tsd.View v = p.GetView() as tsd.View;

                    WorkPlaneHandler wph = new Model().GetWorkPlaneHandler();
                    TransformationPlane tp = wph.GetCurrentTransformationPlane();
                    TransformationPlane tp0 = new TransformationPlane(v.DisplayCoordinateSystem);
                    wph.SetCurrentTransformationPlane(tp0);

                    ModelObject mobj = new Model().SelectModelObject(p.ModelIdentifier);

                    Point min = null;
                    Point max = null;

                    if (mobj is Part)
                    {
                        min = (mobj as Part).GetSolid().MinimumPoint;
                        max = (mobj as Part).GetSolid().MaximumPoint;
                    }

                    if (mobj is RebarGroup)
                    {
                        min = (mobj as RebarGroup).GetSolid().MinimumPoint;
                        max = (mobj as RebarGroup).GetSolid().MaximumPoint;
                    }

                    if (mobj is SingleRebar)
                    {
                        min = (mobj as SingleRebar).GetSolid().MinimumPoint;
                        max = (mobj as SingleRebar).GetSolid().MaximumPoint;
                    }

                    //if (mobj is RebarGroup)
                    try
                    {
                        double scale = Convert.ToDouble(textBox3.Text);

                        Point labelPoint = new Point(min.X + 2*scale, min.Y + 2*scale, min.Z + 2 * scale);

                        Point CenterPoint = new Point(
                            (min.X + max.X) / 2 - scale,
                            (min.Y + max.Y) / 2 - scale,
                            (min.Z + max.Z) / 2 - scale);

                        max = new Point(max.X + scale, max.Y + scale, max.Z + scale);

                        tsd.DetailMark sm = new tsd.DetailMark(v, CenterPoint, max, labelPoint);

                       // MessageBox.Show(sm.BoundaryPoint.ToString() + Environment.NewLine + sm.CenterPoint.ToString());

                        sm.Attributes.MarkName = textBox1.Text;
                        sm.Attributes.BoundaryShape = tsd.DetailMark.DetailMarkAttributes.DetailBoundaryShape.Rectangular;

                        sm.Attributes.BoundingLine.Type = tsd.LineTypes.DashDoubleDot;
                        sm.Attributes.BoundingLine.Color = tsd.DrawingColors.Yellow;

                        tsd.TextElement te = new tsd.TextElement(textBox2.Text);
                        te.Font.Name = "Gost 2.304 type A";
                        te.Font.Height = 3.5;

                        sm.Attributes.TagsAttributes.TagA2.TagContent.Clear();
                        sm.Attributes.TagsAttributes.TagA2.TagContent.Add(te);

                        sm.Insert();


                    }
                    catch (Exception ex){ MessageBox.Show(ex.ToString()); }
                    finally
                    {
                        wph.SetCurrentTransformationPlane(tp);
                    }
                }
            }
            dh.GetActiveDrawing().CommitChanges();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            circle(1);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            CreateDetailView();
        }
    }
}
