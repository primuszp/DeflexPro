namespace DeflexPro.Model
{
    public class PavementLayer
    {
        public string MaterialName { get; set; } = "AC";
        public double Thickness { get; set; } = 150;   // mm; 0 = halfspace
        public double SeedModulus { get; set; } = 3000; // MPa
        public double MinModulus { get; set; } = 100;
        public double MaxModulus { get; set; } = 50000;
        public bool IsFixed { get; set; } = false;
        public bool IsHalfspace { get; set; } = false;

        public PavementLayer Clone() => new PavementLayer
        {
            MaterialName = MaterialName,
            Thickness = Thickness,
            SeedModulus = SeedModulus,
            MinModulus = MinModulus,
            MaxModulus = MaxModulus,
            IsFixed = IsFixed,
            IsHalfspace = IsHalfspace
        };
    }
}
