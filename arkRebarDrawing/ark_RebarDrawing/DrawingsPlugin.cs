using System;
using System.Windows.Forms;
using System.Collections.Generic;

using Tekla.Structures.Plugins;
using Tekla.Structures.Drawing;
using Tekla.Structures.Drawing.Tools;
using Tekla.Structures.Drawing.UI;
using tsd = Tekla.Structures.Drawing;
using Tekla.Structures.Datatype;
using Tekla.Structures.Geometry3d;
using System.Collections;
using tsm = Tekla.Structures.Model;
using System.Globalization;

namespace ark_RebarDrawing
{
    public class PluginData
    {
        #region Fields
        //
        // Define the fields specified on the Form.
        //
        /* Some examples:
        [Tekla.Structures.Plugins.StructuresField("text")]
        public string Text;

        [Tekla.Structures.Plugins.StructuresField("color")]
        public int Color;

        [Tekla.Structures.Plugins.StructuresField("angle")]
        public double Angle;
        */
        #endregion
    }

    [Plugin("ark_RebarDrawing")]
    [PluginUserInterface("ark_RebarDrawing.arkRebarDrawingForm")]
    [UpdateMode(UpdateMode.INPUT_CHANGED)]
    public class ark_RebarDrawing : DrawingPluginBase
    {
        #region Fields
        private PluginData data;
        //
        // Define variables for the field values.
        //
        #endregion

        #region Properties
        private PluginData Data
        {
            get { return data; }
            set { data = value; }
        }
        #endregion

        #region Constructor
        public ark_RebarDrawing(PluginData data)
        {
            Data = data;
        }
        #endregion

        #region Overrides
        public override List<InputDefinition> DefineInput()
        {
            //
            // This is an example for selecting two points; change this to suit your needs.
            //
            List<InputDefinition> inputs = new List<InputDefinition>();
            DrawingHandler drawingHandler = new DrawingHandler();

            if (drawingHandler.GetConnectionStatus())
            {
                Picker picker = drawingHandler.GetPicker();

                ViewBase view = null;
                //PointList points = new PointList();
                DrawingObject dobj = null;

                StringList prompts = new StringList();
                picker.PickObject("Выберите арматурную группу", out dobj, out view);
                //picker.PickPoints(2, prompts, out points, out view);
                inputs.Add(InputDefinitionFactory.CreateInputDefinition(view, dobj));
            }

            return inputs;
        }

        public override bool Run(List<InputDefinition> inputs)
        {
            try
            {
                //
                // This is an example for selecting two points; change this to suit your needs.
                //
                ViewBase view = InputDefinitionFactory.GetView(inputs[0]);
                DrawingObject dobj = InputDefinitionFactory.GetDrawingObject(inputs[0]);
                dobj.Select();

                ReinforcementGroup rgs = dobj as ReinforcementGroup;
               
                tsd.View vw = view as tsd.View;
               // Matrix m = MatrixFactory.ToCoordinateSystem(vw.DisplayCoordinateSystem);

                tsm.ModelObject mrg = new tsm.Model().SelectModelObject(new Tekla.Structures.Identifier(rgs.ModelIdentifier.ID));

                tsm.RebarGroup rg = mrg as tsm.RebarGroup;

                string result = "";

                rg.GetReportProperty("CC_EXACT", ref result);

                DistanceList distanceList1 = new DistanceList();
                ArrayList dist = new ArrayList();
                distanceList1 = DistanceList.Parse(result, CultureInfo.InvariantCulture, Tekla.Structures.Datatype.Distance.UnitType.Millimeter);
                foreach (Tekla.Structures.Datatype.Distance distance in distanceList1)
                    dist.Add(distance.ConvertTo(Tekla.Structures.Datatype.Distance.UnitType.Millimeter));
                dist.RemoveAt(0);

                Point min = (rg.GetSolid().MinimumPoint);
                Point max = (rg.GetSolid().MaximumPoint);

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
            catch (Exception Exc)
            {
                MessageBox.Show(Exc.ToString());
            }

            return true;
        }
        #endregion

        #region Private methods
        
        private void core()
        {
            
        }

        #endregion
    }
}
