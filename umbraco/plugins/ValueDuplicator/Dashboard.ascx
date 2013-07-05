<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Dashboard.ascx.cs" Inherits="ValueDuplicator.Dashboard" %>

<h1>Copy node data values</h1>

<asp:DropDownList ID="DoctypesList" runat="server" AutoPostBack="true" onselectedindexchanged="DoctypesList_Changed" CssClass="bigInput"></asp:DropDownList>

<asp:PlaceHolder ID="FieldsPanel" runat="server" Visible="false">
    <div class="propertypane" style="margin-top:20px;">
        <div style="width:50%;float:left;">
            <h3>Source: <asp:DropDownList ID="PropertiesList" runat="server" AutoPostBack="true" onselectedindexchanged="PropertiesList_Changed"></asp:DropDownList></h3>
            <asp:PlaceHolder ID="PropertyInfo" runat="server" Visible="false">
                <p>
                    Alias: <asp:Literal ID="piAlias" runat="server"></asp:Literal>
                    <br />
                    Data Type: <asp:Literal ID="piType" runat="server"></asp:Literal>
                </p>
            </asp:PlaceHolder>
        </div>
        <div style="width:40%;float:left;">
            <asp:PlaceHolder ID="TargetPropertyPanel" runat="server">
                <div>
                    <h3>Target: <asp:DropDownList ID="TargetProperty" runat="server" AutoPostBack="true" onselectedindexchanged="TargetProperty_Changed"></asp:DropDownList></h3>
                    <asp:PlaceHolder ID="TargetPropertyInfo" runat="server" Visible="false">
                        <p>
                            Alias: <asp:Literal ID="tpAlias" runat="server"></asp:Literal>
                            <br />
                            Data Type: <asp:Literal ID="tpType" runat="server"></asp:Literal>
                        </p>
                    </asp:PlaceHolder>
                </div>
            </asp:PlaceHolder>
        </div>
        <div style="clear:both;"></div>
    </div>

    <asp:PlaceHolder ID="DifferentDatatypeWarning" runat="server" Visible="false">
        <p style="color:red;">
            Please make sure that the copied values will be compatible with the target data type.
        </p>
    </asp:PlaceHolder>

</asp:PlaceHolder>

<asp:PlaceHolder ID="CopyProcess" runat="server" Visible="false">

    <div class="propertypane" style="margin-top:20px;" id="ValueDuplicatorCopyStartPanel">
        <p>
            <asp:Button ID="StartCopy" runat="server" Text="Copy" CssClass="bigInput" style="width:auto;padding:0px 10px;" OnClick="StartCopy_Click" OnClientClick="$('#ValueDuplicatorCopySpinner').show();$('#StartCopy').hide();" ClientIDMode="Static" />
            <img src="/Umbraco/Images/throbber.gif" style="display:none;" id="ValueDuplicatorCopySpinner" />
        </p>
        <div ID="CopiedNodes" runat="server">
        </div>
    </div>

</asp:PlaceHolder>