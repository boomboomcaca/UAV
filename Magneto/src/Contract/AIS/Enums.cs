using System.ComponentModel;

namespace Magneto.Contract.AIS;

/// <summary>
///     航行状态
/// </summary>
public enum NavigationState
{
    /// <summary>
    ///     机航中
    /// </summary>
    [Description("机航中")] UnderWayUsingEngine = 0,

    /// <summary>
    ///     锚泊
    /// </summary>
    [Description("锚泊")] AtAnchor = 1,

    /// <summary>
    ///     未操纵
    /// </summary>
    [Description("未操纵")] NotUnderCommand = 2,

    /// <summary>
    ///     操纵受限
    /// </summary>
    [Description("操纵受限")] RestrictedManoeuverAbility = 3,

    /// <summary>
    ///     吃水受限
    /// </summary>
    [Description("吃水受限")] ConstrainedByItsDraught = 4,

    /// <summary>
    ///     靠泊
    /// </summary>
    [Description("靠泊")] Moored = 5,

    /// <summary>
    ///     搁浅
    /// </summary>
    [Description("搁浅")] Aground = 6,

    /// <summary>
    ///     从事捕捞
    /// </summary>
    [Description("从事捕捞")] EngagedInFishing = 7,

    /// <summary>
    ///     风帆提供动力
    /// </summary>
    [Description("风帆提供动力")] UnderWaySailing = 8,

    /// <summary>
    ///     为HSC保留
    /// </summary>
    [Description("为HSC保留")] ReservedForFutureAmendmentForHsc = 9,

    /// <summary>
    ///     为WIG保留
    /// </summary>
    [Description("为WIG保留")] ReservedForFutureAmendmentForWig = 10,

    /// <summary>
    ///     保留
    /// </summary>
    [Description("保留")] ReservedForFuture1 = 11,

    /// <summary>
    ///     保留
    /// </summary>
    [Description("保留")] ReservedForFuture2 = 12,

    /// <summary>
    ///     保留
    /// </summary>
    [Description("保留")] ReservedForFuture3 = 13,

    /// <summary>
    ///     AIS SART(最新的搜救应答器/发射设备)
    /// </summary>
    [Description("AIS SART(最新的搜救应答器/发射设备)")]
    AisSartIsActive = 14,

    /// <summary>
    ///     未定义，默认值
    /// </summary>
    [Description("未定义")] Undefined
}

/// <summary>
///     船舶类型
/// </summary>
public enum ShipType
{
    /// <summary>
    ///     不可用
    /// </summary>
    [Description("不可用")] NotAvailable = 0,

    /// <summary>
    ///     保留为定义
    /// </summary>
    [Description("保留未定义")] Reserved = 1,

    /// <summary>
    ///     地效翼船
    /// </summary>
    [Description("地效翼船")] WingInGround = 2,

    /// <summary>
    ///     特殊类型船舶
    /// </summary>
    [Description("特殊类型船舶")] SpecialCategory = 3,

    /// <summary>
    ///     高速船舶
    /// </summary>
    [Description("高速船舶")] HighSpeedCraft = 4,

    /// <summary>
    ///     客船
    /// </summary>
    [Description("客船")] Passenger = 6,

    /// <summary>
    ///     货船
    /// </summary>
    [Description("货船")] Cargo = 7,

    /// <summary>
    ///     油轮
    /// </summary>
    [Description("油轮")] Tanker = 8,

    /// <summary>
    ///     其他船舶
    /// </summary>
    [Description("其他船舶")] Other = 9,

    /// <summary>
    ///     保留未定义 10-19
    /// </summary>
    [Description("保留未定义")] ReservedUnspecified = 10,

    /// <summary>
    ///     所有类型的地效翼船
    /// </summary>
    [Description("地效翼船")] WigAllShipsOfThisType = 20,

    /// <summary>
    ///     A类地效翼船
    /// </summary>
    [Description("A类地效翼船")] WigHazardousCategoryA = 21,

    /// <summary>
    ///     B类地效翼船
    /// </summary>
    [Description("B类地效翼船")] WigHazardousCategoryB = 22,

    /// <summary>
    ///     C类地效翼船
    /// </summary>
    [Description("C类地效翼船")] WigHazardousCategoryC = 23,

    /// <summary>
    ///     D类地效翼船
    /// </summary>
    [Description("D类地效翼船")] WigHazardousCategoryD = 24,

    /// <summary>
    ///     地效翼船将来使用
    /// </summary>
    [Description("地效翼船将来使用")] WigReservedForFutureUse = 25, //25-29

    /// <summary>
    ///     捕捞
    /// </summary>
    [Description("捕捞")] Fishing = 30,

    /// <summary>
    ///     牵引船
    /// </summary>
    [Description("牵引船")] Towing = 31,

    /// <summary>
    ///     拖带长度超过200m或宽度超过25m的牵引船
    /// </summary>
    [Description("拖带长度超过200m或宽度超过25m的牵引船")]
    TowingLengthExceeds200MOrBreadthExceeds25M = 32,

    /// <summary>
    ///     清淤水下行动
    /// </summary>
    [Description("清淤水下行动")] DredgingOrUnderwaterOps = 33,

    /// <summary>
    ///     潜水行动
    /// </summary>
    [Description("潜水行动")] DivingOps = 34,

    /// <summary>
    ///     军事行动
    /// </summary>
    [Description("军事行动")] MilitaryOps = 35,

    /// <summary>
    ///     帆船
    /// </summary>
    [Description("帆船")] Sailing = 36,

    /// <summary>
    ///     游艇
    /// </summary>
    [Description("游艇")] PleasureCraft = 37,

    /// <summary>
    ///     保留
    /// </summary>
    [Description("保留")] Reserved3 = 38, //38-39

    /// <summary>
    ///     所有类型的高速船
    /// </summary>
    [Description("高速船")] HscAllShipsOfThisType = 40,

    /// <summary>
    ///     危险级别A的高速船
    /// </summary>
    [Description("危险级别A的高速船")] HscHazardousCategoryA = 41,

    /// <summary>
    ///     危险级别B的高速船
    /// </summary>
    [Description("危险级别C的高速船")] HscHazardousCategoryB = 42,

    /// <summary>
    ///     危险级别C的高速船
    /// </summary>
    [Description("危险级别C的高速船")] HscHazardousCategoryC = 43,

    /// <summary>
    ///     危险级别D的高速船
    /// </summary>
    [Description("危险级别D的高速船")] HscHazardousCategoryD = 44,

    /// <summary>
    ///     高速船将来使用
    /// </summary>
    [Description("高速船将来使用")] HscReservedForFutureUse = 45, //45-48

    /// <summary>
    ///     没有附加信息的高速船
    /// </summary>
    [Description("没有附加信息的高速船")] HscNoAdditionalInformation = 49,

    /// <summary>
    ///     引航船
    /// </summary>
    [Description("引航船")] PilotVessel = 50,

    /// <summary>
    ///     搜救船
    /// </summary>
    [Description("搜救船")] SearchAndRescueVessel = 51,

    /// <summary>
    ///     拖船
    /// </summary>
    [Description("拖船")] Tug = 52,

    /// <summary>
    ///     港口补给船
    /// </summary>
    [Description("港口补给船")] PortTender = 53,

    /// <summary>
    ///     防污船
    /// </summary>
    [Description("防污船")] AntiPollutionEquipment = 54,

    /// <summary>
    ///     执法船
    /// </summary>
    [Description("执法船")] LawEnforcement = 55,

    /// <summary>
    ///     备用的本地船只
    /// </summary>
    [Description("备用的本地船只")] SpareLocalVessel = 56, //56-57

    /// <summary>
    ///     医疗运送船
    /// </summary>
    [Description("医疗运送船")] MedicalTransport = 58,

    /// <summary>
    ///     符合RR18号决议的船
    /// </summary>
    [Description("符合RR18号决议的船")] ShipAccordingToRrResolutionNo18 = 59,

    /// <summary>
    ///     所有类型的客船
    /// </summary>
    [Description("客船")] PassengerAllShipsOfThisType = 60,

    /// <summary>
    ///     A类危险客船
    /// </summary>
    [Description("A类危险客船")] PassengerHazardousCategoryA = 61,

    /// <summary>
    ///     B类危险客船
    /// </summary>
    [Description("B类危险客船")] PassengerHazardousCategoryB = 62,

    /// <summary>
    ///     C类危险客船
    /// </summary>
    [Description("C类危险客船")] PassengerHazardousCategoryC = 63,

    /// <summary>
    ///     D类危险客船
    /// </summary>
    [Description("D类危险客船")] PassengerHazardousCategoryD = 64,

    /// <summary>
    ///     将来使用的客船类型
    /// </summary>
    [Description("将来使用的客船类型")] PassengerReservedForFutureUse = 65, //65-68

    /// <summary>
    ///     没有附加信息的客船
    /// </summary>
    [Description("没有附加信息的客船")] PassengerNoAdditionalInformation = 69,

    /// <summary>
    ///     所有类型的货船
    /// </summary>
    [Description("货船")] CargoAllShipsOfThisType = 70,

    /// <summary>
    ///     A类危险货船
    /// </summary>
    [Description("A类危险货船")] CargoHazardousCategoryA = 71,

    /// <summary>
    ///     B类危险货船
    /// </summary>
    [Description("B类危险货船")] CargoHazardousCategoryB = 72,

    /// <summary>
    ///     C类危险货船
    /// </summary>
    [Description("C类危险货船")] CargoHazardousCategoryC = 73,

    /// <summary>
    ///     D类危险货船
    /// </summary>
    [Description("D类危险货船")] CargoHazardousCategoryD = 74,

    /// <summary>
    ///     将来使用的货船类型
    /// </summary>
    [Description("将来使用的货船类型")] CargoReservedForFutureUse = 75, //75-78

    /// <summary>
    ///     没有附加信息的货船类型
    /// </summary>
    [Description("没有附加信息的货船类型")] CargoNoAdditionalInformation = 79,

    /// <summary>
    ///     所有类型的油轮
    /// </summary>
    [Description("油轮")] TankerAllShipsofThisType = 80,

    /// <summary>
    ///     载运A类危险货物的油轮
    /// </summary>
    [Description("载运A类危险货物的油轮")] TankerHazardousCategoryA = 81,

    /// <summary>
    ///     载运B类危险货物的油轮
    /// </summary>
    [Description("载运B类危险货物的油轮")] TankerHazardousCategoryB = 82,

    /// <summary>
    ///     载运C类危险货物的油轮
    /// </summary>
    [Description("载运C类危险货物的油轮")] TankerHazardousCategoryC = 83,

    /// <summary>
    ///     载运D类危险货物的油轮
    /// </summary>
    [Description("载运D类危险货物的油轮")] TankerHazardousCategoryD = 84,

    /// <summary>
    ///     将来使用的油轮类型
    /// </summary>
    [Description("将来使用的油轮类型")] TankerReservedForFutureUse = 85, //85-88

    /// <summary>
    ///     没有附加信息的油轮类型
    /// </summary>
    [Description("没有附加信息的油轮类型")] TankerNoAdditionalInformation = 89,

    /// <summary>
    ///     所有类型的其他船舶
    /// </summary>
    [Description("所有类型的其他船舶")] OtherTypeAllShipsOfThisType = 90,

    /// <summary>
    ///     载运A类危险物的其他船舶
    /// </summary>
    [Description("载运A类危险物的其他船舶")] OtherTypeHazardousCategoryA = 91,

    /// <summary>
    ///     载运B类危险物的其他船舶
    /// </summary>
    [Description("载运B类危险物的其他船舶")] OtherTypeHazardousCategoryB = 92,

    /// <summary>
    ///     载运C类危险物的其他船舶
    /// </summary>
    [Description("载运C类危险物的其他船舶")] OtherTypeHazardousCategoryC = 93,

    /// <summary>
    ///     载运D类危险物的其他船舶
    /// </summary>
    [Description("载运D类危险物的其他船舶")] OtherTypeHazardousCategoryD = 94,

    /// <summary>
    ///     为将来保留的其他类型船舶
    /// </summary>
    [Description("为将来保留的其他类型船舶")] OtherTypeReservedForFutureUse = 95, //95=98

    /// <summary>
    ///     没有附加信息的其他船舶
    /// </summary>
    [Description("没有附加信息的其他船舶")] OtherTypeNoAdditionalInformation = 99,

    /// <summary>
    ///     位置船舶类型
    /// </summary>
    [Description("未知船舶类型")] UnKnown
}

/// <summary>
///     国家ID
/// </summary>
public enum CountryId
{
    /// <summary>
    ///     Albania_Republic_of 201
    /// </summary>
    [Description("阿尔巴尼亚共和国")] AlbaniaRepublicOf = 201,

    /// <summary>
    ///     Andorra_Principality_of 202
    /// </summary>
    [Description("安道尔公国")] AndorraPrincipalityOf = 202,

    /// <summary>
    ///     Austria 203
    /// </summary>
    [Description("奥地利")] Austria = 203,

    /// <summary>
    ///     Azores 204
    /// </summary>
    [Description("亚速尔群岛")] Azores = 204,

    /// <summary>
    ///     Belgium 205
    /// </summary>
    [Description("比利时")] Belgium = 205,

    /// <summary>
    ///     Belarus_Republic_of 206
    /// </summary>
    [Description("白俄罗斯")] BelarusRepublicOf = 206,

    /// <summary>
    ///     Bulgaria_Republic_of 207
    /// </summary>
    [Description("保加利亚共和国")] BulgariaRepublicOf = 207,

    /// <summary>
    ///     Vatican_City_State 208
    /// </summary>
    [Description("梵蒂冈城国")] VaticanCityState = 208,

    /// <summary>
    ///     Cyprus_Republic_of 209 210 212
    /// </summary>
    [Description("塞浦路斯共和国")] CyprusRepublicOf = 209,

    /// <summary>
    ///     Germany_Federal_Republicof 211 218
    /// </summary>
    [Description("德意志联邦共和国")] GermanyFederalRepublicof = 211,

    /// <summary>
    ///     Georgia 213
    /// </summary>
    [Description("佐治亚州")] Georgia = 213,

    /// <summary>
    ///     Moldova_Republic_of 214
    /// </summary>
    [Description("摩尔多瓦共和国")] MoldovaRepublicOf = 214,

    /// <summary>
    ///     Malta 215 248 249 256
    /// </summary>
    [Description("马耳他")] Malta = 215,

    /// <summary>
    ///     Armenia_Republic_of 216
    /// </summary>
    [Description("亚美尼亚共和国")] ArmeniaRepublicOf = 216,

    /// <summary>
    ///     Denmark 219 220
    /// </summary>
    [Description("丹麦")] Denmark = 219,

    /// <summary>
    ///     Spain 224 225
    /// </summary>
    [Description("西班牙")] Spain = 224,

    /// <summary>
    ///     France 226 227 228
    /// </summary>
    [Description("法国")] France = 226,

    /// <summary>
    ///     Finland 230
    /// </summary>
    [Description("芬兰")] Finland = 230,

    /// <summary>
    ///     Faroe_Islands 231
    /// </summary>
    [Description("法罗群岛")] FaroeIslands = 231,

    /// <summary>
    ///     United_Kingdom_of_Great_Britain_and_Northern_Ireland 232 233 234 235
    /// </summary>
    [Description("大不列颠及北爱尔兰联合王国")] UnitedKingdomOfGreatBritainAndNorthernIreland = 232,

    /// <summary>
    ///     Gibraltar 236
    /// </summary>
    [Description("直布罗陀")] Gibraltar = 236,

    /// <summary>
    ///     Greece 237 239 240 241
    /// </summary>
    [Description("希腊")] Greece = 237,

    /// <summary>
    ///     Croatia_Republic_of 238
    /// </summary>
    [Description("克罗地亚共和国")] CroatiaRepublicOf = 238,

    /// <summary>
    ///     Morocco_Kingdom_of 242
    /// </summary>
    [Description("摩洛哥王国")] MoroccoKingdomOf = 242,

    /// <summary>
    ///     Hungary_Republic_of 243
    /// </summary>
    [Description("匈牙利共和国")] HungaryRepublicOf = 243,

    /// <summary>
    ///     Netherlands_Kingdom_of_the 244 245 246
    /// </summary>
    [Description("荷兰王国")] NetherlandsKingdomOfThe = 244,

    /// <summary>
    ///     Italy 247
    /// </summary>
    [Description("意大利")] Italy = 247,

    /// <summary>
    ///     Ireland 250
    /// </summary>
    [Description("爱尔兰")] Ireland = 250,

    /// <summary>
    ///     Iceland 251
    /// </summary>
    [Description("冰岛")] Iceland = 251,

    /// <summary>
    ///     Liechtenstein_Principality_of 252
    /// </summary>
    [Description("列支敦士登公国")] LiechtensteinPrincipalityOf = 252,

    /// <summary>
    ///     Luxembourg 253
    /// </summary>
    [Description("卢森堡")] Luxembourg = 253,

    /// <summary>
    ///     Monaco_Principality_of 254
    /// </summary>
    [Description("摩纳哥公国")] MonacoPrincipalityOf = 254,

    /// <summary>
    ///     Madeira 255
    /// </summary>
    [Description("马德拉")] Madeira = 255,

    /// <summary>
    ///     Norway 257 258 259
    /// </summary>
    [Description("挪威")] Norway = 257,

    /// <summary>
    ///     Poland_Republic_of 261
    /// </summary>
    [Description("波兰共和国")] PolandRepublicOf = 261,

    /// <summary>
    ///     Montenegro 262
    /// </summary>
    [Description("黑山共和国")] Montenegro = 262,

    /// <summary>
    ///     Portugal 263
    /// </summary>
    [Description("葡萄牙")] Portugal = 263,

    /// <summary>
    ///     Romania 264
    /// </summary>
    [Description("罗马尼亚")] Romania = 264,

    /// <summary>
    ///     Sweden 265 266
    /// </summary>
    [Description("瑞典")] Sweden = 265,

    /// <summary>
    ///     Slovak_Republic 267
    /// </summary>
    [Description("斯洛伐克共和国")] SlovakRepublic = 267,

    /// <summary>
    ///     San_Marino_Republic_of 268
    /// </summary>
    [Description("圣马力诺共和国")] SanMarinoRepublicOf = 268,

    /// <summary>
    ///     Switzerland_Confederation_of 269
    /// </summary>
    [Description("瑞士联邦")] SwitzerlandConfederationOf = 269,

    /// <summary>
    ///     Czech_Republic 270
    /// </summary>
    [Description("捷克共和国")] CzechRepublic = 270,

    /// <summary>
    ///     Turkey 271
    /// </summary>
    [Description("土耳其")] Turkey = 271,

    /// <summary>
    ///     Ukraine 272
    /// </summary>
    [Description("乌克兰")] Ukraine = 272,

    /// <summary>
    ///     Russian_Federation 273
    /// </summary>
    [Description("俄罗斯")] RussianFederation = 273,

    /// <summary>
    ///     TheFormer_Yugoslav_Republic_of_Macedonia 274
    /// </summary>
    [Description("前南斯拉夫的马其顿共和国")] TheFormerYugoslavRepublicOfMacedonia = 274,

    /// <summary>
    ///     Latvia_Republic_of 275
    /// </summary>
    [Description("拉脱维亚共和国")] LatviaRepublicOf = 275,

    /// <summary>
    ///     Estonia_Republic_of 276
    /// </summary>
    [Description("爱沙尼亚共和国")] EstoniaRepublicOf = 276,

    /// <summary>
    ///     Lithuania_Republic_of 277
    /// </summary>
    [Description("立陶宛共和国")] LithuaniaRepublicOf = 277,

    /// <summary>
    ///     Slovenia_Republic_of 278
    /// </summary>
    [Description("斯洛文尼亚共和国")] SloveniaRepublicOf = 278,

    /// <summary>
    ///     Serbia_Republic_of 279
    /// </summary>
    [Description("塞尔维亚共和国")] SerbiaRepublicOf = 279,

    /// <summary>
    ///     Anguilla 301
    /// </summary>
    [Description("安圭拉")] Anguilla = 301,

    /// <summary>
    ///     Alaska_State_of 303
    /// </summary>
    [Description("阿拉斯加州")] AlaskaStateOf = 303,

    /// <summary>
    ///     Antigua_and_Barbuda 304 305
    /// </summary>
    [Description("安提瓜和巴布达")] AntiguaAndBarbuda = 304,

    /// <summary>
    ///     Netherlands_Antilles 306
    /// </summary>
    [Description("荷属安的列斯")] NetherlandsAntilles = 306,

    /// <summary>
    ///     Aruba 307
    /// </summary>
    [Description("阿鲁巴")] Aruba = 307,

    /// <summary>
    ///     Bahamas_Commonwealth_of_the 308 309 311
    /// </summary>
    [Description("巴哈马国")] BahamasCommonwealthOfThe = 308,

    /// <summary>
    ///     Bermuda 310
    /// </summary>
    [Description("百慕大")] Bermuda = 310,

    /// <summary>
    ///     Belize 312
    /// </summary>
    [Description("伯利兹")] Belize = 312,

    /// <summary>
    ///     Barbados 314
    /// </summary>
    [Description("巴巴多斯")] Barbados = 314,

    /// <summary>
    ///     Canada 316
    /// </summary>
    [Description("加拿大")] Canada = 316,

    /// <summary>
    ///     Cayman_Islands 319
    /// </summary>
    [Description("开曼群岛")] CaymanIslands = 319,

    /// <summary>
    ///     Costa_Rica 321
    /// </summary>
    [Description("哥斯达黎加")] CostaRica = 321,

    /// <summary>
    ///     Cuba 323
    /// </summary>
    [Description("古巴")] Cuba = 323,

    /// <summary>
    ///     Dominica_Commonwealth_of 325
    /// </summary>
    [Description("多米尼加联邦")] DominicaCommonwealthOf = 325,

    /// <summary>
    ///     Dominican_Republic 327
    /// </summary>
    [Description("多米尼加共和国")] DominicanRepublic = 327,

    /// <summary>
    ///     Guadeloupe_French_Department_of 329
    /// </summary>
    [Description("瓜德罗普岛的法语系")] GuadeloupeFrenchDepartmentOf = 329,

    /// <summary>
    ///     Grenada 330
    /// </summary>
    [Description("格林纳达")] Grenada = 330,

    /// <summary>
    ///     Greenland 331
    /// </summary>
    [Description("格陵兰")] Greenland = 331,

    /// <summary>
    ///     Guatemala_Republicof 332
    /// </summary>
    [Description("危地马拉共和国")] GuatemalaRepublicof = 332,

    /// <summary>
    ///     Honduras_Republic_of 334
    /// </summary>
    [Description("洪都拉斯共和国")] HondurasRepublicOf = 334,

    /// <summary>
    ///     Haiti_Republicof 336
    /// </summary>
    [Description("海地共和国")] HaitiRepublicof = 336,

    /// <summary>
    ///     United_States_of_America 338 366 367 368 369
    /// </summary>
    [Description("美国")] UnitedStatesOfAmerica = 338,

    /// <summary>
    ///     Jamaica 339
    /// </summary>
    [Description("牙买加")] Jamaica = 339,

    /// <summary>
    ///     Saint_Kitts_and_Nevis_Federation_of 341
    /// </summary>
    [Description("圣基茨和尼维斯联邦")] SaintKittsAndNevisFederationOf = 341,

    /// <summary>
    ///     Saint_Lucia 343
    /// </summary>
    [Description("圣露西亚")] SaintLucia = 343,

    /// <summary>
    ///     Mexico 345
    /// </summary>
    [Description("墨西哥")] Mexico = 345,

    /// <summary>
    ///     Martinique_French_Department_of 347
    /// </summary>
    [Description("马提尼克岛的法语系")] MartiniqueFrenchDepartmentOf = 347,

    /// <summary>
    ///     Montserrat 348
    /// </summary>
    [Description("蒙特塞拉特")] Montserrat = 348,

    /// <summary>
    ///     Nicaragua 350
    /// </summary>
    [Description("尼加拉瓜")] Nicaragua = 350,

    /// <summary>
    ///     Panama_Republic_of 351 352 353 354 370 371 372
    /// </summary>
    [Description("巴拿马共和国")] PanamaRepublicOf = 351,

    /// <summary>
    ///     - 355356 357
    /// </summary>
    [Description("未定义")] UnDefined = 355,

    /// <summary>
    ///     Puerto_Rico 358
    /// </summary>
    [Description("波多黎各")] PuertoRico = 358,

    /// <summary>
    ///     El_Salvador_Republic_of 359
    /// </summary>
    [Description("萨尔瓦多共和国")] ElSalvadorRepublicOf = 359,

    /// <summary>
    ///     Saint_Pierre_and_Miquelon_Territorial_Collectivity_of 361
    /// </summary>
    [Description("圣皮埃尔和密克隆群岛")] SaintPierreAndMiquelonTerritorialCollectivityOf = 361,

    /// <summary>
    ///     Trinidad_and_Tobago 362
    /// </summary>
    [Description("特立尼达和多巴哥")] TrinidadAndTobago = 362,

    /// <summary>
    ///     Turks_and_Caicos_Islands 364
    /// </summary>
    [Description("特克斯和凯科斯群岛")] TurksAndCaicosIslands = 364,

    /// <summary>
    ///     Saint_Vincent_and_the_Grenadines 375 376 377
    /// </summary>
    [Description("圣文森特和格林纳丁斯")] SaintVincentAndTheGrenadines = 375,

    /// <summary>
    ///     British_Virgin_Islands 378
    /// </summary>
    [Description("英属维尔京群岛")] BritishVirginIslands = 378,

    /// <summary>
    ///     UnitedStates_Virgin_Islands 379
    /// </summary>
    [Description("美属维尔京群岛")] UnitedStatesVirginIslands = 379,

    /// <summary>
    ///     Afghanistan 401
    /// </summary>
    [Description("阿富汗")] Afghanistan = 401,

    /// <summary>
    ///     Saudi_Arabia_Kingdom_of 403
    /// </summary>
    [Description("沙特阿拉伯王国")] SaudiArabiaKingdomOf = 403,

    /// <summary>
    ///     Bangladesh_People's_Republic_of 405
    /// </summary>
    [Description("孟加拉人民共和国")] BangladeshPeoplesRepublicOf = 405,

    /// <summary>
    ///     Bahrain_Kingdom_of 408
    /// </summary>
    [Description("巴林王国")] BahrainKingdomOf = 408,

    /// <summary>
    ///     Bhutan_Kingdomof 410
    /// </summary>
    [Description("不丹王国")] BhutanKingdomof = 410,

    /// <summary>
    ///     China_People'sRepublic_of 412 413
    /// </summary>
    [Description("中国")] PeoplesRepublicOfChina = 412,

    /// <summary>
    ///     Taiwan_Province_of_China 416
    /// </summary>
    [Description("中国台湾省")] TaiwanProvinceOfChina = 416,

    /// <summary>
    ///     Sri_Lanka_Democratic_Socialist_Republicof 417
    /// </summary>
    [Description("斯里兰卡民主社会主义共和国")] SriLankaDemocraticSocialistRepublicof = 417,

    /// <summary>
    ///     India_Republicof 419
    /// </summary>
    [Description("印度")] IndiaRepublicof = 419,

    /// <summary>
    ///     Iran_IslamicRepublic_of 422
    /// </summary>
    [Description("伊朗")] IranIslamicRepublicOf = 422,

    /// <summary>
    ///     Azerbaijani_Republic 423
    /// </summary>
    [Description("阿塞拜疆共和国")] AzerbaijaniRepublic = 423,

    /// <summary>
    ///     Iraq_Republicof 425
    /// </summary>
    [Description("伊拉克")] IraqRepublicof = 425,

    /// <summary>
    ///     Israel_Stateof 428
    /// </summary>
    [Description("以色列")] IsraelStateof = 428,

    /// <summary>
    ///     Japan 431 432
    /// </summary>
    [Description("日本")] Japan = 431,

    /// <summary>
    ///     Turkmenistan 434
    /// </summary>
    [Description("土库曼斯坦")] Turkmenistan = 434,

    /// <summary>
    ///     Kazakhstan_Republic_of 436
    /// </summary>
    [Description("哈萨克斯坦共和国")] KazakhstanRepublicOf = 436,

    /// <summary>
    ///     Uzbekistan_Republic_of 437
    /// </summary>
    [Description("乌兹别克斯坦共和国")] UzbekistanRepublicOf = 437,

    /// <summary>
    ///     Jordan_Hashemite_Kingdomof 438
    /// </summary>
    [Description("约旦哈希姆王国")] JordanHashemiteKingdomof = 438,

    /// <summary>
    ///     Korea_Republicof 440 441
    /// </summary>
    [Description("韩国")] KoreaRepublicof = 440,

    /// <summary>
    ///     Palestine_In_accordancewith_Resolution_99_Rev_Antalya_2006 443
    /// </summary>
    [Description("巴勒斯坦按照99号决议启安塔利亚2006")] PalestineInAccordancewithResolution99RevAntalya2006 = 443,

    /// <summary>
    ///     DemocraticPeople's_Republic_of_Korea 445
    /// </summary>
    [Description("朝鲜人民民主共和国")] DemocraticPeoplesRepublicOfKorea = 445,

    /// <summary>
    ///     Kuwait_Stateof 447
    /// </summary>
    [Description("科威特州")] KuwaitStateof = 447,

    /// <summary>
    ///     Lebanon 450
    /// </summary>
    [Description("黎巴嫩")] Lebanon = 450,

    /// <summary>
    ///     Kyrgyz_Republic 451
    /// </summary>
    [Description("吉尔吉斯共和国")] KyrgyzRepublic = 451,

    /// <summary>
    ///     Macao_Special_Administrative_Region_of_China 453
    /// </summary>
    [Description("中国澳门特别行政区")] MacaoSpecialAdministrativeRegionOfChina = 453,

    /// <summary>
    ///     Maldives_Republic_of 455
    /// </summary>
    [Description("马尔代夫共和国")] MaldivesRepublicOf = 455,

    /// <summary>
    ///     Mongolia 457
    /// </summary>
    [Description("蒙古")] Mongolia = 457,

    /// <summary>
    ///     Nepal_FederalDemocratic_Republic_of 459
    /// </summary>
    [Description("尼泊尔联邦民主共和国")] NepalFederalDemocraticRepublicOf = 459,

    /// <summary>
    ///     Oman_Sultanateof 461
    /// </summary>
    [Description("阿曼苏丹国")] OmanSultanateof = 461,

    /// <summary>
    ///     Pakistan_Islamic_Republic_of 463
    /// </summary>
    [Description("巴基斯坦伊斯兰共和国")] PakistanIslamicRepublicOf = 463,

    /// <summary>
    ///     Qatar_Stateof 466
    /// </summary>
    [Description("卡塔尔国")] QatarStateof = 466,

    /// <summary>
    ///     Syrian_Arab_Republic 468
    /// </summary>
    [Description("阿拉伯叙利亚共和国")] SyrianArabRepublic = 468,

    /// <summary>
    ///     United_Arab_Emirates 470
    /// </summary>
    [Description("阿拉伯联合酋长国")] UnitedArabEmirates = 470,

    /// <summary>
    ///     Yemen_Republicof 473 475
    /// </summary>
    [Description("也门共和国")] YemenRepublicof = 473,

    /// <summary>
    ///     HongKong_Special_Administrative_Region_of_China 477
    /// </summary>
    [Description("中国香港特别行政区")] HongKongSpecialAdministrativeRegionOfChina = 477,

    /// <summary>
    ///     Bosnia_and_Herzegovina 478
    /// </summary>
    [Description("波斯尼亚和黑塞哥维那")] BosniaAndHerzegovina = 478,

    /// <summary>
    ///     AdelieLand 501
    /// </summary>
    [Description("阿德利土地")] AdelieLand = 501,

    /// <summary>
    ///     Australia 503
    /// </summary>
    [Description("澳大利亚")] Australia = 503,

    /// <summary>
    ///     Myanmar_Unionof 506
    /// </summary>
    [Description("缅甸联盟")] MyanmarUnionof = 506,

    /// <summary>
    ///     BruneiDarussalam 508
    /// </summary>
    [Description("文莱达鲁萨兰国")] BruneiDarussalam = 508,

    /// <summary>
    ///     Micronesia_Federated_States_of 510
    /// </summary>
    [Description("密克罗尼西亚联邦")] MicronesiaFederatedStatesOf = 510,

    /// <summary>
    ///     Palau_Republicof 511
    /// </summary>
    [Description("帕劳共和国")] PalauRepublicof = 511,

    /// <summary>
    ///     New_Zealand 512
    /// </summary>
    [Description("新西兰")] NewZealand = 512,

    /// <summary>
    ///     Cambodia_Kingdom_of 514 515
    /// </summary>
    [Description("柬埔寨王国")] CambodiaKingdomOf = 514,

    /// <summary>
    ///     ChristmasIsland_Indian_Ocean 516
    /// </summary>
    [Description("圣诞岛印度洋")] ChristmasIslandIndianOcean = 516,

    /// <summary>
    ///     Cook_Islands 518
    /// </summary>
    [Description("库克群岛")] CookIslands = 518,

    /// <summary>
    ///     Fiji_Republicof 520
    /// </summary>
    [Description("斐济共和国")] FijiRepublicof = 520,

    /// <summary>
    ///     Cocos_Keeling_Islands 523
    /// </summary>
    [Description("科科斯(基林)群岛")] CocosKeelingIslands = 523,

    /// <summary>
    ///     Indonesia_Republic_of 525
    /// </summary>
    [Description("印度尼西亚")] IndonesiaRepublicOf = 525,

    /// <summary>
    ///     Kiribati_Republic_of 529
    /// </summary>
    [Description("基里巴斯共和国")] KiribatiRepublicOf = 529,

    /// <summary>
    ///     LaoPeople's_Democratic_Republic 531
    /// </summary>
    [Description("老挝人民民主共和国")] LaoPeoplesDemocraticRepublic = 531,

    /// <summary>
    ///     Malaysia 533
    /// </summary>
    [Description("马来西亚")] Malaysia = 533,

    /// <summary>
    ///     Northern_Mariana_Islands_Commonwealth_of_the 536
    /// </summary>
    [Description("北马里亚纳群岛联邦")] NorthernMarianaIslandsCommonwealthOfThe = 536,

    /// <summary>
    ///     Marshall_Islands_Republic_of_the 538
    /// </summary>
    [Description("马绍尔群岛")] MarshallIslandsRepublicOfThe = 538,

    /// <summary>
    ///     New_Caledonia 540
    /// </summary>
    [Description("新喀里多尼亚")] NewCaledonia = 540,

    /// <summary>
    ///     Niue 542
    /// </summary>
    [Description("纽埃")] Niue = 542,

    /// <summary>
    ///     Nauru_Republicof 544
    /// </summary>
    [Description("瑙鲁共和国")] NauruRepublicof = 544,

    /// <summary>
    ///     French_Polynesia 546
    /// </summary>
    [Description("法属波利尼西亚")] FrenchPolynesia = 546,

    /// <summary>
    ///     Philippines_Republic_of_the 548
    /// </summary>
    [Description("菲律宾共和国")] PhilippinesRepublicOfThe = 548,

    /// <summary>
    ///     Papua_New_Guinea 553
    /// </summary>
    [Description("巴布亚新几内亚")] PapuaNewGuinea = 553,

    /// <summary>
    ///     Pitcairn_Island 555
    /// </summary>
    [Description("皮特凯恩岛")] PitcairnIsland = 555,

    /// <summary>
    ///     Solomon_Islands 557
    /// </summary>
    [Description("所罗门群岛")] SolomonIslands = 557,

    /// <summary>
    ///     American_Samoa 559
    /// </summary>
    [Description("美属萨摩亚")] AmericanSamoa = 559,

    /// <summary>
    ///     Samoa_Independent_State_of 561
    /// </summary>
    [Description("萨摩亚独立国")] SamoaIndependentStateOf = 561,

    /// <summary>
    ///     Singapore_Republic_of 563 564 565
    /// </summary>
    [Description("新加坡共和国")] SingaporeRepublicOf = 563,

    /// <summary>
    ///     Thailand 567
    /// </summary>
    [Description("泰国")] Thailand = 567,

    /// <summary>
    ///     Tonga_Kingdomof 570
    /// </summary>
    [Description("汤加王国")] TongaKingdomof = 570,

    /// <summary>
    ///     Tuvalu 572
    /// </summary>
    [Description("图瓦卢")] Tuvalu = 572,

    /// <summary>
    ///     Viet_Nam_Socialist_Republicof 574
    /// </summary>
    [Description("越南社会主义共和国")] VietNamSocialistRepublicof = 574,

    /// <summary>
    ///     Vanuatu_Republic_of 576
    /// </summary>
    [Description("瓦努阿图共和国")] VanuatuRepublicOf = 576,

    /// <summary>
    ///     Wallis_and_Futuna_Islands 578
    /// </summary>
    [Description("瓦利斯和富图纳群岛")] WallisAndFutunaIslands = 578,

    /// <summary>
    ///     South_Africa_Republic_of 601
    /// </summary>
    [Description("南非共和国")] SouthAfricaRepublicOf = 601,

    /// <summary>
    ///     Angola_Republic_of 603
    /// </summary>
    [Description("安哥拉共和国")] AngolaRepublicOf = 603,

    /// <summary>
    ///     Algeria_People's_Democratic_Republic_of 605
    /// </summary>
    [Description("阿尔及利亚民主人民共和国")] AlgeriaPeoplesDemocraticRepublicOf = 605,

    /// <summary>
    ///     Saint_Paul_and_Amsterdam_Islands 607
    /// </summary>
    [Description("圣保罗和阿姆斯特丹群岛")] SaintPaulAndAmsterdamIslands = 607,

    /// <summary>
    ///     Ascension_Island 608
    /// </summary>
    [Description("阿森松岛")] AscensionIsland = 608,

    /// <summary>
    ///     Burundi_Republic_of 609
    /// </summary>
    [Description("布隆迪共和国")] BurundiRepublicOf = 609,

    /// <summary>
    ///     Benin_Republicof 610
    /// </summary>
    [Description("贝宁共和国")] BeninRepublicof = 610,

    /// <summary>
    ///     Botswana_Republic_of 611
    /// </summary>
    [Description("博茨瓦纳共和国")] BotswanaRepublicOf = 611,

    /// <summary>
    ///     Central_African_Republic 612
    /// </summary>
    [Description("中非共和国")] CentralAfricanRepublic = 612,

    /// <summary>
    ///     Cameroon_Republic_of 613
    /// </summary>
    [Description("喀麦隆共和国")] CameroonRepublicOf = 613,

    /// <summary>
    ///     Congo_Republicof_the 615
    /// </summary>
    [Description("刚果共和国")] CongoRepublicofThe = 615,

    /// <summary>
    ///     Comoros_Unionof_the 616
    /// </summary>
    [Description("科摩罗联盟")] ComorosUnionofThe = 616,

    /// <summary>
    ///     Cape_Verde_Republic_of 617
    /// </summary>
    [Description("佛得角共和国")] CapeVerdeRepublicOf = 617,

    /// <summary>
    ///     CrozetArchipelago 618
    /// </summary>
    [Description("克罗泽群岛")] CrozetArchipelago = 618,

    /// <summary>
    ///     Côte d'Ivoire_Republicof 619
    /// </summary>
    [Description("科特迪瓦共和国")] CoteDIvoireRepublicof = 619,

    /// <summary>
    ///     Djibouti_Republic_of 621
    /// </summary>
    [Description("吉布提共和国")] DjiboutiRepublicOf = 621,

    /// <summary>
    ///     Egypt_Arab_Republicof 622
    /// </summary>
    [Description("埃及阿拉伯共和国")] EgyptArabRepublicof = 622,

    /// <summary>
    ///     Ethiopia_Federal_Democratic_Republic_of 624
    /// </summary>
    [Description("埃塞俄比亚联邦民主共和国")] EthiopiaFederalDemocraticRepublicOf = 624,

    /// <summary>
    ///     Eritrea 625
    /// </summary>
    [Description("厄立特里亚")] Eritrea = 625,

    /// <summary>
    ///     Gabonese_Republic 626
    /// </summary>
    [Description("加蓬共和国")] GaboneseRepublic = 626,

    /// <summary>
    ///     Ghana 627
    /// </summary>
    [Description("加纳")] Ghana = 627,

    /// <summary>
    ///     Gambia_Republic_of_the 629
    /// </summary>
    [Description("冈比亚共和国")] GambiaRepublicOfThe = 629,

    /// <summary>
    ///     Guinea-Bissau_Republic_of 630
    /// </summary>
    [Description("几内亚比绍共和国")] GuineaBissauRepublicOf = 630,

    /// <summary>
    ///     Equatorial_Guinea_Republic_of 631
    /// </summary>
    [Description("赤道几内亚共和国")] EquatorialGuineaRepublicOf = 631,

    /// <summary>
    ///     Guinea_Republic_of 632
    /// </summary>
    [Description("几内亚共和国")] GuineaRepublicOf = 632,

    /// <summary>
    ///     Burkina_Faso 633
    /// </summary>
    [Description("布基纳法索")] BurkinaFaso = 633,

    /// <summary>
    ///     Kenya_Republicof 634
    /// </summary>
    [Description("肯尼亚共和国")] KenyaRepublicof = 634,

    /// <summary>
    ///     Kerguelen_Islands 635
    /// </summary>
    [Description("凯尔盖朗群岛")] KerguelenIslands = 635,

    /// <summary>
    ///     Liberia_Republic_of 636 637
    /// </summary>
    [Description("利比里亚共和国")] LiberiaRepublicOf = 636,

    /// <summary>
    ///     SocialistPeople's_Libyan_Arab_Jamahiriya 642
    /// </summary>
    [Description("利比亚")] SocialistPeoplesLibyanArabJamahiriya = 642,

    /// <summary>
    ///     Lesotho_Kingdom_of 644
    /// </summary>
    [Description("莱索托王国")] LesothoKingdomOf = 644,

    /// <summary>
    ///     Mauritius_Republic_of 645
    /// </summary>
    [Description("毛里求斯共和国")] MauritiusRepublicOf = 645,

    /// <summary>
    ///     Madagascar_Republic_of 647
    /// </summary>
    [Description("马达加斯加共和国")] MadagascarRepublicOf = 647,

    /// <summary>
    ///     Mali_Republicof 649
    /// </summary>
    [Description("马里共和国")] MaliRepublicof = 649,

    /// <summary>
    ///     Mozambique_Republic_of 650
    /// </summary>
    [Description("莫桑比克共和国")] MozambiqueRepublicOf = 650,

    /// <summary>
    ///     Mauritania_Islamic_Republic_of 654
    /// </summary>
    [Description("毛里塔尼亚伊斯兰共和国")] MauritaniaIslamicRepublicOf = 654,

    /// <summary>
    ///     Malawi 655
    /// </summary>
    [Description("马拉维")] Malawi = 655,

    /// <summary>
    ///     Niger_Republicof_the 656
    /// </summary>
    [Description("尼日尔共和国")] NigerRepublicofThe = 656,

    /// <summary>
    ///     Nigeria_Federal_Republicof 657
    /// </summary>
    [Description("尼日利亚联邦共和国")] NigeriaFederalRepublicof = 657,

    /// <summary>
    ///     Namibia_Republic_of 659
    /// </summary>
    [Description("纳米比亚共和国")] NamibiaRepublicOf = 659,

    /// <summary>
    ///     Reunion_French_Department_of 660
    /// </summary>
    [Description("留尼汪岛的法语系")] ReunionFrenchDepartmentOf = 660,

    /// <summary>
    ///     Rwanda_Republic_of 661
    /// </summary>
    [Description("卢旺达共和国")] RwandaRepublicOf = 661,

    /// <summary>
    ///     Sudan_Republicof_the 662
    /// </summary>
    [Description("苏丹共和国")] SudanRepublicofThe = 662,

    /// <summary>
    ///     Senegal_Republic_of 663
    /// </summary>
    [Description("塞内加尔共和国")] SenegalRepublicOf = 663,

    /// <summary>
    ///     Seychelles_Republic_of 664
    /// </summary>
    [Description("塞舌尔共和国")] SeychellesRepublicOf = 664,

    /// <summary>
    ///     Saint_Helena 665
    /// </summary>
    [Description("圣赫勒拿")] SaintHelena = 665,

    /// <summary>
    ///     SomaliDemocratic_Republic 666
    /// </summary>
    [Description("索马里民主共和国")] SomaliDemocraticRepublic = 666,

    /// <summary>
    ///     Sierra_Leone 667
    /// </summary>
    [Description("塞拉利昂")] SierraLeone = 667,

    /// <summary>
    ///     Sao_Tome_and_Principe_Democratic_Republic_of 668
    /// </summary>
    [Description("圣多美和普林西比")] SaoTomeAndPrincipeDemocraticRepublicOf = 668,

    /// <summary>
    ///     Swaziland_Kingdom_of 669
    /// </summary>
    [Description("斯威士兰王国")] SwazilandKingdomOf = 669,

    /// <summary>
    ///     Chad_Republicof 670
    /// </summary>
    [Description("乍得共和国")] ChadRepublicof = 670,

    /// <summary>
    ///     Togolese_Republic 671
    /// </summary>
    [Description("多哥共和国")] TogoleseRepublic = 671,

    /// <summary>
    ///     Tunisia 672
    /// </summary>
    [Description("突尼斯")] Tunisia = 672,

    /// <summary>
    ///     Tanzania_United_Republic_of 674 677
    /// </summary>
    [Description("坦桑尼亚联合共和国")] TanzaniaUnitedRepublicOf = 674,

    /// <summary>
    ///     Uganda_Republic_of 675
    /// </summary>
    [Description("乌干达共和国")] UgandaRepublicOf = 675,

    /// <summary>
    ///     Democratic_Republic_of_the_Congo 676
    /// </summary>
    [Description("刚果民主共和国")] DemocraticRepublicOfTheCongo = 676,

    /// <summary>
    ///     Zambia_Republic_of 678
    /// </summary>
    [Description("赞比亚共和国")] ZambiaRepublicOf = 678,

    /// <summary>
    ///     Zimbabwe_Republic_of 679
    /// </summary>
    [Description("津巴布韦共和国")] ZimbabweRepublicOf = 679,

    /// <summary>
    ///     Argentine_Republic 701
    /// </summary>
    [Description("阿根廷共和国")] ArgentineRepublic = 701,

    /// <summary>
    ///     Brazil_Federative_Republic_of 710
    /// </summary>
    [Description("巴西联邦共和国")] BrazilFederativeRepublicOf = 710,

    /// <summary>
    ///     Bolivia_Plurinational_Stateof 720
    /// </summary>
    [Description("玻利维亚多民族国")] BoliviaPlurinationalStateof = 720,

    /// <summary>
    ///     Chile 725
    /// </summary>
    [Description("智利")] Chile = 725,

    /// <summary>
    ///     Colombia_Republic_of 730
    /// </summary>
    [Description("哥伦比亚共和国")] ColombiaRepublicOf = 730,

    /// <summary>
    ///     Ecuador 735
    /// </summary>
    [Description("厄瓜多尔")] Ecuador = 735,

    /// <summary>
    ///     Falkland_Islands_Malvinas 740
    /// </summary>
    [Description("福克兰群岛马尔维纳斯")] FalklandIslandsMalvinas = 740,

    /// <summary>
    ///     Guiana_French_Department_of 745
    /// </summary>
    [Description("法属圭亚那的法语系")] GuianaFrenchDepartmentOf = 745,

    /// <summary>
    ///     Guyana 750
    /// </summary>
    [Description("圭亚那")] Guyana = 750,

    /// <summary>
    ///     Paraguay_Republic_of 755
    /// </summary>
    [Description("巴拉圭共和国")] ParaguayRepublicOf = 755,

    /// <summary>
    ///     Peru 760
    /// </summary>
    [Description("秘鲁")] Peru = 760,

    /// <summary>
    ///     Suriname_Republic_of 765
    /// </summary>
    [Description("苏里南共和国")] SurinameRepublicOf = 765,

    /// <summary>
    ///     Uruguay_Eastern_Republic_of 770
    /// </summary>
    [Description("乌拉圭共和国")] UruguayEasternRepublicOf = 770,

    /// <summary>
    ///     Venezuela_Bolivarian_Republicof 775
    /// </summary>
    [Description("委内瑞拉玻利瓦尔共和国")] VenezuelaBolivarianRepublicof = 775,

    /// <summary>
    ///     未知国家
    /// </summary>
    [Description("未知国家")] UnKnown
}

/// <summary>
///     消息类型
/// </summary>
public enum AisMessageType
{
    /// <summary>
    ///     未知消息类型
    /// </summary>
    [Description("未知消息类型")] UnKnown = 0,

    /// <summary>
    ///     移动台定期报告位置
    /// </summary>
    [Description("移动台定期报告位置")] MobileStationPosition = 1,

    /// <summary>
    ///     AIS 基站位置报告
    /// </summary>
    [Description("AIS 基站位置报告")] AisBaseSatationReport = 4,

    /// <summary>
    ///     船舶静态和航行相关数据
    /// </summary>
    [Description("船舶静态和航行相关数据")] VesselStaticAndVoyage = 5,

    /// <summary>
    ///     标准B类船舶位置报告
    /// </summary>
    [Description("标准B类船舶位置报告")] StandardBPosition = 18,

    /// <summary>
    ///     扩展的B类船舶位置信息
    /// </summary>
    [Description("扩展的B类船舶位置信息")] ExtensionBPosition = 19,

    /// <summary>
    ///     静态数据报告（A,B）
    /// </summary>
    [Description("静态数据报告（A,B）")] StaticDataReport = 24
}