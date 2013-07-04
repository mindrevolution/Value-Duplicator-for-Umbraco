using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using umbraco.cms.businesslogic.web;
using umbraco.cms.businesslogic.propertytype;

namespace ValueDuplicator
{
    public partial class Dashboard : System.Web.UI.UserControl
    {
        private ListItem chooseItemFromList = new ListItem("Choose ...", string.Empty);

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                DoctypesList.Items.Add(chooseItemFromList);
                foreach (DocumentType doctype in umbraco.cms.businesslogic.web.DocumentType.GetAllAsList())
                {
                    DoctypesList.Items.Add(new ListItem(doctype.Text + " (" + doctype.Alias + ")", doctype.Id.ToString()));
                }
            }
        }

        protected void DoctypesList_Changed(object sender, EventArgs e)
        {
            // - there is a doctype selected
            if (!string.IsNullOrEmpty(DoctypesList.SelectedValue))
            {
                DocumentType doctype = new umbraco.cms.businesslogic.web.DocumentType(System.Convert.ToInt32(DoctypesList.SelectedValue));
                if (doctype != null)
                {
                    PropertiesList.Items.Clear();
                    PropertiesList.Items.Add(chooseItemFromList);
                    foreach (PropertyType prop in doctype.PropertyTypes)
                    {
                        PropertiesList.Items.Add(new ListItem(prop.Name + " (" + prop.Alias + ")", prop.Id.ToString()));
                    }
                }

                FieldsPanel.Visible = (doctype != null);
                TargetPropertyPanel.Visible = !FieldsPanel.Visible;
            }
            else
            {
                FieldsPanel.Visible = false;
            }
        }

        protected void PropertiesList_Changed(object sender, EventArgs e)
        {
            // - there is a property selected
            if (!string.IsNullOrEmpty(PropertiesList.SelectedValue))
            {
                PropertyType prop = new PropertyType(Convert.ToInt32(PropertiesList.SelectedValue));

                if (prop != null)
                {
                    // - property info
                    piAlias.Text = prop.Alias;
                    piType.Text = prop.DataTypeDefinition.DataType.DataTypeName;

                    // - fill target property list
                    DocumentType doctype = new umbraco.cms.businesslogic.web.DocumentType(System.Convert.ToInt32(DoctypesList.SelectedValue));
                    if (doctype != null)
                    {
                        TargetProperty.Items.Clear();
                        TargetProperty.Items.Add(chooseItemFromList);
                        foreach (PropertyType pi in doctype.PropertyTypes)
                        {
                            // - don't add originating property!
                            if (pi.Id != prop.Id)
                                TargetProperty.Items.Add(new ListItem(pi.Name + " (" + pi.Alias + ")", pi.Id.ToString()));
                        }
                    }

                    PropertyInfo.Visible = true;
                    TargetPropertyPanel.Visible = true;
                    TargetPropertyInfo.Visible = false;
                }
            }
            else
            {
                TargetPropertyPanel.Visible = false;
                TargetPropertyInfo.Visible = false;
            }
        }

        protected void TargetProperty_Changed(object sender, EventArgs e)
        {
            // - there is a target property selected
            if (!string.IsNullOrEmpty(TargetProperty.SelectedValue))
            {
                PropertyType prop = new PropertyType(Convert.ToInt32(PropertiesList.SelectedValue));
                PropertyType targetprop = new PropertyType(Convert.ToInt32(TargetProperty.SelectedValue));

                if (targetprop != null)
                {
                    // - property info
                    tpAlias.Text = targetprop.Alias;
                    tpType.Text = targetprop.DataTypeDefinition.DataType.DataTypeName;

                    // - no of same type as source? show warning!
                    DifferentDatatypeWarning.Visible = (prop.DataTypeDefinition.DataType.Id != targetprop.DataTypeDefinition.DataType.Id);

                    TargetPropertyInfo.Visible = true;
                    CopyProcess.Visible = true;
                }
            }
            else
            {
                TargetPropertyInfo.Visible = false;
            }

        }

        protected void StartCopy_Click(object sender, EventArgs e)
        {
            // - get all documents of this type
            DocumentType doctype = new umbraco.cms.businesslogic.web.DocumentType(System.Convert.ToInt32(DoctypesList.SelectedValue));
            umbraco.cms.businesslogic.Content[] docs = Document.getContentOfContentType(doctype);

            Document doc;
            foreach (umbraco.cms.businesslogic.Content d in docs)
            {
                doc = new Document(d.Id);
                CopiedNodes.InnerText += doc.Text + "--" + doc.Id.ToString() + "--";
            }
        }
    }
}