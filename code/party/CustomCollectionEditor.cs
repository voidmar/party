using System;
using System.ComponentModel.Design;
using System.Windows.Forms;

namespace party
{
    interface ICareAboutChanges
    {
        void NotifyChanged();
    }

    class CustomCollectionEditor: CollectionEditor
    {
        public CustomCollectionEditor(Type type) : base(type)
        {
        }

        protected override CollectionForm CreateCollectionForm()
        {
            CollectionForm base_form = base.CreateCollectionForm();
            TableLayoutPanel table_layout = base_form.Controls[0] as TableLayoutPanel;
            if (table_layout != null)
            {
                PropertyGrid property_grid = table_layout.Controls[5] as PropertyGrid;
                if (property_grid != null)
                {
                    property_grid.PropertySort = PropertySort.NoSort;
                    property_grid.ToolbarVisible = false;
                }
            }
            return base_form; // you saw nothing
        }

        public override object EditValue(System.ComponentModel.ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            object modified_value = base.EditValue(context, provider, value);
            if (modified_value is ICareAboutChanges) ((ICareAboutChanges)modified_value).NotifyChanged();
            return modified_value;
        }
    }
}
