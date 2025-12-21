---
trigger: always_on
description: ProxySwitcher縺ｮ繝励Ο繧ｸ繧ｧ繧ｯ繝医さ繝ｳ繝・く繧ｹ繝医→髢狗匱繝ｫ繝ｼ繝ｫ
---

# ProxySwitcher 繧ｫ繧ｹ繧ｿ繝繧ｨ繝ｼ繧ｸ繧ｧ繝ｳ繝郁ｦ丞援

縺薙・繝輔ぃ繧､繝ｫ縺ｯ縲￣roxySwitcher 繝ｪ繝昴ず繝医Μ繧呈桶縺・お繝ｼ繧ｸ繧ｧ繝ｳ繝医・縺溘ａ縺ｮ繧ｬ繧､繝峨Λ繧､繝ｳ縺ｨ繝励Ο繧ｸ繧ｧ繧ｯ繝医さ繝ｳ繝・く繧ｹ繝医ｒ螳夂ｾｩ縺励∪縺吶・
## 1. 繝励Ο繧ｸ繧ｧ繧ｯ繝域ｦりｦ・ProxySwitcher 縺ｯ縲仝indows 縺ｮ繝励Ο繧ｭ繧ｷ險ｭ螳壹ｒ繧ｿ繧ｹ繧ｯ繝医Ξ繧､縺九ｉ邏譌ｩ縺丞・繧頑崛縺医ｋ縺溘ａ縺ｮ霆ｽ驥上い繝励Μ繧ｱ繝ｼ繧ｷ繝ｧ繝ｳ縺ｧ縺吶・# (.NET 9 / Windows Forms) 縺ｧ螳溯｣・＆繧後※縺・∪縺吶・
## 2. 繧ｳ繧｢讖溯・
- **謇句虚蛻・ｊ譖ｿ縺・*: 繧ｿ繧ｹ繧ｯ繝医Ξ繧､繝｡繝九Η繝ｼ縺ｾ縺溘・繝帙ャ繝医く繝ｼ (Ctrl + Alt + P) 縺ｫ繧医ｋ蜊ｳ譎ょ・繧頑崛縺医・- **WiFi騾｣蜍戊・蜍募喧**: 謗･邯壻ｸｭ縺ｮ SSID 縺梧欠螳壹Μ繧ｹ繝医↓蜷ｫ縺ｾ繧後ｋ蝣ｴ蜷医∬・蜍慕噪縺ｫ繝励Ο繧ｭ繧ｷ繧呈怏蜉ｹ蛹悶・- **WiFi繧ｹ繧ｭ繝｣繝ｳ & 謇句虚霑ｽ蜉**: 蜻ｨ蝗ｲ縺ｮ繝阪ャ繝医Ρ繝ｼ繧ｯ繧偵せ繧ｭ繝｣繝ｳ縺励※驕ｸ謚槭√∪縺溘・ SSID 繧呈焔蜍募・蜉帙＠縺ｦ繝ｪ繧ｹ繝育匳骭ｲ縲・- **蜍慕噪繧｢繧､繧ｳ繝ｳ**: 繝励Ο繧ｭ繧ｷ縺ｮ ON/OFF 迥ｶ諷九ｒ蜿肴丐縺励◆繧ｰ繝ｩ繝輔ぅ繧ｫ繝ｫ縺ｪ繧｢繧､繧ｳ繝ｳ繧貞虚逧・↓逕滓・縲・
## 3. 繝・ぅ繝ｬ繧ｯ繝医Μ繝輔ぃ繧､繝ｫ讒区・
- **Program.cs**: 繧ｨ繝ｳ繝医Μ繝昴う繝ｳ繝医ゅヨ繝ｬ繧､繧｢繧､繧ｳ繝ｳ縲√さ繝ｳ繝・く繧ｹ繝医Γ繝九Η繝ｼ縲√・繝・ヨ繧ｭ繝ｼ縺翫ｈ縺ｳ逶｣隕悶け繝ｩ繧ｹ縺ｮ繝ｩ繧､繝輔し繧､繧ｯ繝ｫ繧堤ｮ｡逅・・- **ProxyManager.cs**: 繝ｬ繧ｸ繧ｹ繝医Μ (HKCU\Software\Microsoft\Windows\CurrentVersion\Internet Settings) 繧呈桃菴懊１roxyEnable 縺ｨ ProxyServer 繧貞宛蠕｡縺励仝ininet API 縺ｧ蜿肴丐縲・- **WifiWatcher.cs**: 繝阪ャ繝医Ρ繝ｼ繧ｯ螟画峩繧堤屮隕悶４SID 繝代・繧ｹ縺ｫ縺ｯ netsh wlan show interfaces 繧剃ｽｿ逕ｨ縲・- **WifiScanner.cs**: netsh wlan show networks 繧剃ｽｿ逕ｨ縺励※蜻ｨ霎ｺ縺ｮ SSID 繧偵Μ繧ｹ繝医い繝・・縲・- **SettingsForm.cs**: 蜷・ｨｮ險ｭ螳啅I縲よ枚蟄怜喧縺代ｒ髦ｲ縺舌◆繧√√ヵ繧ｩ繝ｳ繝医・ Yu Gothic UI 9pt 蝗ｺ螳壹ゅさ繝ｳ繝医Ο繝ｼ繝ｫ縺ｮ驟咲ｽｮ縺ｯ繝槭・繧ｸ繝ｳ縺ｫ菴呵｣輔ｒ謖√▽縺薙→縲・- **AppConfig.cs**: %AppData%\ProxySwitcher\config.json 縺ｫ險ｭ螳壹ｒ豌ｸ邯壼喧縲・- **HotKeyHandler.cs**: Win32 API (RegisterHotKey) 繧剃ｽｿ逕ｨ縺励◆繧ｷ繧ｹ繝・Β蜈ｨ菴薙〒縺ｮ繧ｷ繝ｧ繝ｼ繝医き繝・ヨ縲・- **AutoStartManager.cs**: 繝ｬ繧ｸ繧ｹ繝医Μ縺ｮ Run 繧ｭ繝ｼ縺ｫ繧医ｋ Windows 襍ｷ蜍墓凾螳溯｡後・邂｡逅・・
## 4. 髢狗匱譎ゅ・驥崎ｦ√↑繝ｫ繝ｼ繝ｫ
- **UI隱ｿ謨ｴ**: 譁・ｭ励・蟠ｩ繧後ｄ驥阪↑繧翫↓髱槫ｸｸ縺ｫ謨乗─縺ｪ縺溘ａ縲∵眠縺励＞繧ｳ繝ｳ繝医Ο繝ｼ繝ｫ繧定ｿｽ蜉縺吶ｋ髫帙・縲∽ｽ呵｣輔ｒ謖√▲縺溘し繧､繧ｺ險ｭ險医→譏守､ｺ逧・↑繝輔か繝ｳ繝域欠螳壹ｒ陦後≧縺薙→縲・- **譁・ｭ励さ繝ｼ繝・*: netsh 繧ｳ繝槭Φ繝峨・邨先棡繧定ｪｭ縺ｿ蜿悶ｋ髫帙・縲√す繧ｹ繝・Β縺ｮ讓呎ｺ匁枚蟄励さ繝ｼ繝峨ｒ菴ｿ逕ｨ縺励∵律譛ｬ隱樒腸蠅・〒縺ｮ繝代・繧ｹ蟠ｩ繧後↓豕ｨ諢上☆繧九％縺ｨ縲・- **髱槫酔譛溷・逅・*: WiFi繧ｹ繧ｭ繝｣繝ｳ縺ｪ縺ｩ縺ｮ譎る俣縺ｮ縺九°繧句・逅・・縲ゞI繝輔Μ繝ｼ繧ｺ繧帝亟縺舌◆繧√↓髱槫酔譛溷喧縺吶ｋ縺薙→縲・
## 5. 蜻ｽ蜷阪→繧ｹ繧ｿ繧､繝ｫ
- 縺吶∋縺ｦ縺ｮ蜃ｺ蜉帙∬ｪｬ譏弱√ラ繧ｭ繝･繝｡繝ｳ繝医・譌･譛ｬ隱槭ゅさ繝ｼ繝峨・繧ｳ繝｡繝ｳ繝医ｂ譌･譛ｬ隱槭・- 邨ｵ譁・ｭ励・菴ｿ逕ｨ縺ｯ蜴ｳ遖√・- 繧ｳ繝溘ャ繝医・繝ｬ繝輔ぅ繝・け繧ｹ繝ｫ繝ｼ繝ｫ・・ix, add, update 遲会ｼ峨ｒ驕ｵ螳医・
## 6. 繝薙Ν繝峨→蜃ｺ蜉・譛ｬ繝励Ο繧ｸ繧ｧ繧ｯ繝医〒縺ｯ縲∝ｸｸ縺ｫ莉･荳九・2遞ｮ鬘槭・螳溯｡後ヵ繧｡繧､繝ｫ縺檎函謌舌＆繧後∪縺吶・
| 遞ｮ鬘・| 蜃ｺ蜉帛・ | 繧ｵ繧､繧ｺ | 迚ｹ蠕ｴ |
| :--- | :--- | :--- | :--- |
| **騾壼ｸｸ迚・(Standard)** | bin\Publish\Standard\ProxySwitcher.exe | 邏・8MB | 繝ｩ繝ｳ繧ｿ繧､繝蜀・鳩縲ゅ％繧御ｸ縺､縺ｧ蜍穂ｽ懊・|
| **霆ｽ驥冗沿 (Lightweight)** | bin\Publish\Lightweight\ProxySwitcher.exe | 邏・.2MB | 繝ｩ繝ｳ繧ｿ繧､繝譛ｪ蜀・鳩縲・NET 9 Desktop Runtime縺悟ｿ・ｦ√・|

### 繝薙Ν繝峨さ繝槭Φ繝・`powershell
# 騾壼ｸｸ迚・dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:PublishTrimmed=false /p:EnableCompressionInSingleFile=true -o bin\Publish\Standard

# 霆ｽ驥冗沿
dotnet publish -c Release -r win-x64 --self-contained false /p:PublishSingleFile=true /p:PublishTrimmed=false /p:EnableCompressionInSingleFile=false -o bin\Publish\Lightweight
`

## 7. AI繧ｨ繝ｼ繧ｸ繧ｧ繝ｳ繝医∈縺ｮ豕ｨ諢丈ｺ矩・- **譁・ｭ励さ繝ｼ繝峨・蜴ｳ螳・*: 縺薙・繝輔ぃ繧､繝ｫ繧堤ｷｨ髮・☆繧矩圀縺ｯ縲∝ｿ・★ **UTF-8 (BOM縺ｪ縺・** 縺ｧ菫晏ｭ倥☆繧九％縺ｨ縲８indows PowerShell 縺ｮ繝・ヵ繧ｩ繝ｫ繝医・ Set-Content 遲峨ｒ菴ｿ逕ｨ縺吶ｋ縺ｨ譁・ｭ怜喧縺代′逋ｺ逕溘＠縲、I繧ｨ繝ｼ繧ｸ繧ｧ繝ｳ繝医・蜍穂ｽ懊↓謾ｯ髫懊ｒ縺阪◆縺吝庄閭ｽ諤ｧ縺後≠繧九◆繧√・NET 縺ｮ WriteAllText 遲峨・驕ｩ蛻・↑繧ｨ繝ｳ繧ｳ繝ｼ繝・ぅ繝ｳ繧ｰ謖・ｮ壹′蜿ｯ閭ｽ縺ｪ謇区ｮｵ繧堤畑縺・ｋ縺薙→縲