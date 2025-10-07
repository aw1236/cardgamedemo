using UnityEngine;
using UnityEngine.EventSystems;

public class EquipmentSlot : CardSlot
{
    [Header("装备槽设置")]
    public GameObject equipEffect;
    public EquipmentType equipmentType; // 装备类型：武器或盔甲

    private void Start()
    {
        // 确保有对主角数据的引用
        if (MainCharacterSlot.Instance == null)
        {
            Debug.LogError("未找到 MainCharacterSlot 实例！");
        }
    }

    // 装备槽允许替换：新卡牌进入时，旧卡牌被替换
    public override void OnDrop(PointerEventData eventData)
    {
        CardDragHandler draggedCard = CardDragHandler.CurrentlyDraggedCard;
        if (draggedCard != null)
        {
            CardView cardView = draggedCard.GetComponent<CardView>();
            CardData originalCardData = cardView.GetCardData();

            if (CanAcceptCard(originalCardData))
            {
                // 🎯 关键修复：创建装备数据的运行时副本
                CardData runtimeCardData = CreateRuntimeCardData(originalCardData);
                cardView.SetCardData(runtimeCardData);

                // 🎯 新增：立即刷新背景（添加这一行）
                cardView.RefreshBackground();

                // 如果槽位已有卡牌，先处理旧卡牌
                if (IsFull())
                {
                    // 🎯 修复：返回旧卡牌到原位置，而不是销毁
                    ReturnCurrentCardToOriginalPosition();
                }

                PlaceCard(draggedCard.transform, cardView);
                Debug.Log($"装备了 {runtimeCardData.cardName} 到 {slotType} 槽位");

                // 🎯 应用装备效果到主角（使用副本数据）
                ApplyEquipmentToMainCharacter(runtimeCardData);

                // 🎯 新增：立即更新装备卡牌的UI显示
                cardView.RefreshDisplay();
            }
            else
            {
                Debug.Log($"无法装备 {originalCardData.cardName} 到 {slotType} 槽位");
                draggedCard.ReturnToOriginalPosition();
            }
        }
    }

    /// <summary>
    /// 🎯 新增：返回当前卡牌到原位置（不销毁）
    /// </summary>
    private void ReturnCurrentCardToOriginalPosition()
    {
        if (CurrentCardView != null)
        {
            // 先移除装备效果
            RemoveEquipmentFromMainCharacter(CurrentCardView.GetCardData());

            // 使用CardDragHandler返回原位置
            CardDragHandler dragHandler = CurrentCardView.GetComponent<CardDragHandler>();
            if (dragHandler != null)
            {
                dragHandler.ReturnToOriginalPosition();
            }
            else
            {
                // 如果没有CardDragHandler，使用安全移除
                SafeRemoveCard();
            }

            CurrentCardView = null;
        }
    }

    /// <summary>
    /// 🎯 关键修复：创建卡牌数据的运行时副本
    /// </summary>
    private CardData CreateRuntimeCardData(CardData originalData)
    {
        if (originalData is WeaponCardData weaponData)
        {
            WeaponCardData runtimeWeapon = ScriptableObject.CreateInstance<WeaponCardData>();
            CopyWeaponData(weaponData, runtimeWeapon);
            return runtimeWeapon;
        }
        else if (originalData is ArmorCardData armorData)
        {
            ArmorCardData runtimeArmor = ScriptableObject.CreateInstance<ArmorCardData>();
            CopyArmorData(armorData, runtimeArmor);
            return runtimeArmor;
        }

        // 其他类型卡牌直接返回原数据
        return originalData;
    }

    /// <summary>
    /// 🎯 关键修复：复制武器数据
    /// </summary>
    private void CopyWeaponData(WeaponCardData source, WeaponCardData target)
    {
        target.cardName = source.cardName;
        target.cardType = source.cardType;
        target.icon = source.icon;
        target.description = source.description;
        target.attack = source.attack;
        target.durability = source.durability; // 使用原始耐久度
        target.maxDurability = source.durability; // 记录最大耐久度

        // 🎯 新增：复制背景预制体引用
        target.cardBackgroundPrefab = source.cardBackgroundPrefab;
    }

    /// <summary>
    /// 🎯 关键修复：复制盔甲数据
    /// </summary>
    private void CopyArmorData(ArmorCardData source, ArmorCardData target)
    {
        target.cardName = source.cardName;
        target.cardType = source.cardType;
        target.icon = source.icon;
        target.description = source.description;
        target.defense = source.defense;
        target.durability = source.durability; // 使用原始耐久度
        target.maxDurability = source.durability; // 记录最大耐久度

        // 🎯 新增：复制背景预制体引用
        target.cardBackgroundPrefab = source.cardBackgroundPrefab;
    }

    protected override void OnCardPlaced(CardView cardView)
    {
        base.OnCardPlaced(cardView);

        // 装备特效
        if (equipEffect != null)
        {
            Instantiate(equipEffect, transform.position, Quaternion.identity, transform);
        }
    }

    protected override void OnCardRemoved(CardView cardView)
    {
        base.OnCardRemoved(cardView);

        // 🎯 移除装备效果
        RemoveEquipmentFromMainCharacter(cardView.GetCardData());
    }

    /// <summary>
    /// 检查是否可以接受该卡牌
    /// </summary>
    public override bool CanAcceptCard(CardData cardData)
    {
        // 首先调用基类的检查
        if (!base.CanAcceptCard(cardData))
            return false;

        // 🎯 根据装备槽类型检查卡牌类型
        switch (equipmentType)
        {
            case EquipmentType.Weapon:
                return cardData is WeaponCardData;
            case EquipmentType.Armor:
                return cardData is ArmorCardData;
            default:
                return false;
        }
    }

    /// <summary>
    /// 将装备应用到主角数据
    /// </summary>
    private void ApplyEquipmentToMainCharacter(CardData cardData)
    {
        if (MainCharacterSlot.Instance == null || MainCharacterSlot.Instance.mainCharacterData == null)
        {
            Debug.LogError("无法应用装备效果：主角数据未找到");
            return;
        }

        var mainChar = MainCharacterSlot.Instance.mainCharacterData;

        if (cardData is WeaponCardData weaponData)
        {
            // 应用武器效果
            mainChar.equippedWeapon = weaponData;
            Debug.Log($"装备武器: {weaponData.cardName} (攻击力+{weaponData.attack})");

            // 更新UI显示
            MainCharacterSlot.Instance.UpdateMainCharacterDisplay();
            MainCharacterSlot.Instance.AddCombatLog($"装备了 {weaponData.cardName}");
        }
        else if (cardData is ArmorCardData armorData)
        {
            // 应用盔甲效果
            mainChar.equippedArmor = armorData;
            Debug.Log($"装备盔甲: {armorData.cardName} (防御力+{armorData.defense})");

            // 更新UI显示
            MainCharacterSlot.Instance.UpdateMainCharacterDisplay();
            MainCharacterSlot.Instance.AddCombatLog($"装备了 {armorData.cardName}");
        }
    }

    /// <summary>
    /// 从主角数据移除装备效果
    /// </summary>
    private void RemoveEquipmentFromMainCharacter(CardData cardData)
    {
        if (MainCharacterSlot.Instance == null || MainCharacterSlot.Instance.mainCharacterData == null)
        {
            Debug.LogError("无法移除装备效果：主角数据未找到");
            return;
        }

        var mainChar = MainCharacterSlot.Instance.mainCharacterData;

        if (cardData is WeaponCardData weaponData)
        {
            // 移除武器效果
            if (mainChar.equippedWeapon == weaponData)
            {
                mainChar.equippedWeapon = null;
                Debug.Log($"卸下武器: {weaponData.cardName}");

                // 更新UI显示
                MainCharacterSlot.Instance.UpdateMainCharacterDisplay();
                MainCharacterSlot.Instance.AddCombatLog($"卸下了 {weaponData.cardName}");
            }
        }
        else if (cardData is ArmorCardData armorData)
        {
            // 移除盔甲效果
            if (mainChar.equippedArmor == armorData)
            {
                mainChar.equippedArmor = null;
                Debug.Log($"卸下盔甲: {armorData.cardName}");

                // 更新UI显示
                MainCharacterSlot.Instance.UpdateMainCharacterDisplay();
                MainCharacterSlot.Instance.AddCombatLog($"卸下了 {armorData.cardName}");
            }
        }
    }

    /// <summary>
    /// 强制移除卡牌（当替换装备时）- 现在改为返回原位置而不是销毁
    /// </summary>
    public override void ForceRemoveCard()
    {
        if (CurrentCardView != null)
        {
            // 先移除装备效果
            RemoveEquipmentFromMainCharacter(CurrentCardView.GetCardData());

            // 🎯 修复：返回原位置而不是销毁
            ReturnCurrentCardToOriginalPosition();
        }
    }

    /// <summary>
    /// 🎯 新增：更新装备卡牌的UI显示
    /// </summary>
    public void UpdateEquipmentDisplay()
    {
        if (CurrentCardView != null)
        {
            CurrentCardView.RefreshDisplay();
        }
    }

    /// <summary>
    /// 🎯 强制移除并销毁卡牌（用于装备损坏时）- 仅在耐久度为0时使用
    /// </summary>
    public void ForceRemoveAndDestroy()
    {
        if (CurrentCardView != null)
        {
            // 先移除装备效果
            RemoveEquipmentFromMainCharacter(CurrentCardView.GetCardData());

            // 销毁卡牌对象
            Destroy(CurrentCardView.gameObject);
            CurrentCardView = null;

            Debug.Log($"已销毁损坏的装备卡牌");
        }
    }
    /// <summary>
    /// 装备类型枚举
    /// </summary>
    public enum EquipmentType
    {
        Weapon,
        Armor
    }
}