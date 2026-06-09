namespace DeflexPro.Model
{
    public class BackcalcResult
    {
        public double StationDistance { get; set; }
        public int DropNumber { get; set; }
        public double[] LayerModuli { get; set; } = [];
        public double SubgradeModulus { get; set; }
        public double RMSE { get; set; }
        public double SCI { get; set; }  // D0 - D300  Surface Curvature Index
        public double BCI { get; set; }  // D600 - D900 Base Curvature Index
        public double BDI { get; set; }  // D300 - D600 Base Damage Index
    }
}
