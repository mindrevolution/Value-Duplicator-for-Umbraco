﻿using System;
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
                Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "ValuesDuplicatorModifyTextbox", "$(function() { $('#ModifyPattern').hide();if(this.checked){$('#ModifyPattern').show()}; });", true);
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
                ModifyValues.Visible = false;
                DifferentDatatypeWarning.Visible = false;
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
                    DifferentDatatypeWarning.Visible = false;
                }
            }
            else
            {
                TargetPropertyPanel.Visible = false;
                TargetPropertyInfo.Visible = false;
                DifferentDatatypeWarning.Visible = false;
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
                    StartCopy.Visible = true;
                    ModifyValues.Visible = true;
                }
            }
            else
            {
                TargetPropertyInfo.Visible = false;
                ModifyValues.Visible = false;
            }

        }

        protected void StartCopy_Click(object sender, EventArgs e)
        {
            // - get all documents of this type
            DocumentType doctype = new umbraco.cms.businesslogic.web.DocumentType(System.Convert.ToInt32(DoctypesList.SelectedValue));
            umbraco.cms.businesslogic.Content[] docs = Document.getContentOfContentType(doctype);

            // - no fancy async stuff (because of ascx), so we need to stretch the environment a bit ...
            Page.Server.ScriptTimeout = (15 * 60); //*60=minutes!

            try
            {
                Document doc;
                string propFromAlias = new PropertyType(Convert.ToInt32(PropertiesList.SelectedValue)).Alias;
                string propToAlias = new PropertyType(Convert.ToInt32(TargetProperty.SelectedValue)).Alias;

                CopiedNodes.InnerHtml = "<ul>";
                foreach (umbraco.cms.businesslogic.Content d in docs)
                {
                    doc = new Document(d.Id);
                    if (doc != null)
                    {
                        // copy value
                        CopyValues(doc, propFromAlias, propToAlias);

                        CopiedNodes.InnerHtml += NodeCopyResultString(doc.Id, doc.Text);
                    }
                    else
                    {
                        CopiedNodes.InnerHtml += "<li>Unable to load document with id " + d.Id.ToString() + "</li>";
                    }
                }
                CopiedNodes.InnerHtml += "</ul>";
            }
            catch { }

            // - back to normal (90 seconds)
            Page.Server.ScriptTimeout = 90;
        }

        private string NodeCopyResultString(object docid, string name)
        {
            return string.Format("<li>Copying field value in document '{0}' (#{1}).</li>", name, docid.ToString());
        }

        // - copies Umbraco document field values
        private void CopyValues(Document doc, string fromProperty, string toProperty, bool multiLang, string multiLangCode)
        {
            //  copy value
            string sourceValue = doc.getProperty(fromProperty).Value.ToString();
            string targetValue = sourceValue;

            // - modify target value?
            if (EnableModifyValues.Checked)
            {
                targetValue = ModifyPattern.Text.Replace("{{VALUE}}", sourceValue);
            }

            // - set new value 
            doc.getProperty(toProperty).Value = targetValue;
            
            // - save (and publish, if requested to do so)
            if (PublishAfterCopy.Checked)
            {
                doc.Save();
            }
            else
            {
                // - v6+ way:
                //doc.SaveAndPublish(umbraco.helper.GetCurrentUmbracoUser());

                // - legacy way
                doc.Save();
                doc.Publish(umbraco.helper.GetCurrentUmbracoUser());
                umbraco.library.UpdateDocumentCache(doc.Id);
            }
        }
        private void CopyValues(Document doc, string fromProperty, string toProperty)
        {
            CopyValues(doc, fromProperty, toProperty, false, string.Empty);
        }

    }
}