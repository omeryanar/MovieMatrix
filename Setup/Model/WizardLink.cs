using System;

namespace Setup.Model
{
    public class WizardLink
    {
        public Uri Source { get; set; }
        public bool AllowBack { get; set; } = true;
        public bool AllowNext { get; set; } = true;
        public bool AllowCancel { get; set; } = true;
        public bool AllowFinish { get; set; }
    }
}
