<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="CalcWeb._Default" %>
              
<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="jumbotron">
        <h1>Range Equilibrium Calculator</h1>
    </div>

    <div class="row">
        <div >
            <h2>Inputs:</h2>
            <p>
                Stacks:&nbsp;&nbsp;&nbsp; 
                <asp:TextBox ID="txtStacks" runat="server" Width="503px">15, 20, 8</asp:TextBox>
            </p>
            <p>
                Payouts:&nbsp;
                <asp:TextBox ID="txtPayouts" runat="server" Width="502px">10</asp:TextBox>
            </p>
            
            <p>
                Blinds:&nbsp;&nbsp;&nbsp;&nbsp;                                                 
                <asp:TextBox ID="txtBlinds" runat="server" Width="499px">1, 2</asp:TextBox>
            </p>
            <p>
                Ante:&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;                                                 
                <asp:TextBox ID="txtAnte" runat="server" Width="499px">0</asp:TextBox>
            </p>
            <p>
                <asp:CheckBox ID="cbUnrestricted" runat="server" Text="Enable unrestricted range calculation?" />
            </p>
            <p>
                <asp:Button ID="btnCalc" runat="server" Text="Calculate!" OnClick="btnCalc_Click" />
            &nbsp; </p>
        </div> 
        
        <div >
            <asp:Literal ID="litResult" runat="server"></asp:Literal>
        </div>
    </div>

</asp:Content>