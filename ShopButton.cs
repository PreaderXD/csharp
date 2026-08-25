	using Terraria;
using TerrariaModder.Core;

public class GuideShop : IMod
{
    public string Id => "guide-shop";
    public string Name => "Guide Shop";
    public string Version => "1.0.0";

    private GuideShopInteraction guideShop;

    public void Initialize(ModContext context)
    {
        guideShop = new GuideShopInteraction();
        Terraria.GameContent.NPCInteractions.All.Add(guideShop);
        context.Logger.Info("Guide Shop loaded!");
    }

    public void Unload()
    {
        if (guideShop != null)
        {
            Terraria.GameContent.NPCInteractions.All.Remove(guideShop);
            guideShop = null;
        }

        Shop.Unload();
    }

    private class GuideShopInteraction : Terraria.GameContent.NPCInteraction
    {
        public override bool Condition()
        {
            return TalkNPCType == 22;
        }

        public override string GetText()
        {
            return "Shop";
        }

        public override void Interact()
        {
            Main.instance.OpenShop(26);
        }
    }
}
