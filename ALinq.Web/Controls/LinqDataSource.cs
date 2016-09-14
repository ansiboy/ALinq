using System.ComponentModel;
using System.Drawing;
using System.IO;

namespace ALinq.Web.Controls
{
    [Designer(typeof(System.Web.UI.Design.WebControls.LinqDataSourceDesigner))]
    [ToolboxBitmap(typeof(ALinqDataSource), "LinqDataSource.ico")]
    public class ALinqDataSource : System.Web.UI.WebControls.LinqDataSource
    {
        protected override System.Web.UI.WebControls.LinqDataSourceView CreateView()
        {
            var result = new ALinqDataSourceView(this, "DefaultView", Context);
            return result;
        }

        [Browsable(false)]
        public TextWriter Log
        {
            get;
            set;
        }

    }
}