# Schema Tools — 데이터 에디터와 명령 팔레트

Schema Tools는 작성자용 창 두 개로, 모드들이 스키마로 담는 설명에서 완전히 만들어집니다(Schemas 모드 참고):

- **데이터 에디터**는 설명된 모든 종류의 엔티티 — 스탯, 클래스, 아이템, 캐릭터, 전투 행동, 상점, 제작법 — 를 양식으로 편집합니다;
- **명령 팔레트**는 양식에서 시나리오 줄을 쓰고, 검사한 뒤 시나리오 에디터에 넣습니다.

두 창 모두 어떤 종류나 행동도 미리 알지 못합니다: 불러온 모드가 설명한 것이 그 안에 나타납니다. 둘 다 게임 도구 모음의 도구 상자에서, 게임이 실행 중일 때 엽니다.

## 데이터 에디터

위에서 모드를, 왼쪽에서 데이터 종류를, 가운데에서 항목을 고르면 양식이 오른쪽에 있습니다. RPG Plus의 Classic RPG Rules 철검은 이렇습니다:

![데이터 에디터: 모드 classic_rpg_pack, 왼쪽에 종류(rpg_tileset 10, Stats 11, Archetypes 3, Equipment slots 3, Items 20, Characters 5, Battle actions 7, Shops 1, Crafting station 1, Recipe 3), 가운데에 그 아이템 아홉 개와 Iron sword 선택, 오른쪽에 양식: Id, Name, Description은 일곱 언어 각각의 텍스트, Cost 30, Picture, Item types TargetUsed, Item action action/equip, Equip type sword, Attack 스탯의 특성.](assets/data-editor-item.jpg)

편집기 그림은 이전 캡처입니다. 보이는 `rpg_demo/` 키나 `rpg_tileset` 팔레트는 구버전이며 본문의 현재 `rpg_plus/` 키와 `rpg_object` 모델을 사용하세요.

양식은 여러 모드의 필드를 하나로 모읍니다: Id, 이름, 설명, 가격, 그림, 아이템 종류와 아이템 행동은 Items Engine이 설명하고, **Equip type**, **Traits** 목록(스탯과 수치), 착용할 수 있는 클래스는 Equipment가 더합니다. 두 모드가 설명하는 종류라도 양식은 하나입니다.

- **텍스트**(`Name`, `Description`)는 번역 키와 모드의 언어별 줄로 표시되며, 모드 자신의 로컬라이즈 파일에 기록됩니다.
- **참조**(스탯, 아이템, 클래스)는 게임이 아는 id의 목록입니다.
- **목록**(traits 등)은 요소마다 카드가 있고 각자 필드를 가집니다.
- 어떤 스키마에도 없는 속성은 보존되어 기타 필드 아래 JSON으로 표시됩니다.

도구 모음: **새로 만들기**(Ctrl+N)는 스키마의 기본값으로 항목을 추가하고, **복사**(Ctrl+D)는 선택 항목을 복제하며, **삭제**는 먼저 묻고, **되돌리기**/**다시 실행**(Ctrl+Z / Ctrl+Y 또는 Ctrl+Shift+Z)은 변경을 오가며, **저장**(Ctrl+S)은 변경된 파일만 LF 줄바꿈으로 기록합니다. 저장한 직후 실행 중인 게임이 변화를 봅니다: 새 아이템을 재시작 없이 주고 착용시킬 수 있습니다.

목록에는 고른 모드의 항목과, **Entries of other mods**를 켜면 의존하는 모드의 항목(`↗ 소유자` 표시)이 나옵니다. 그런 항목을 편집하면 고른 모드에 `patches/<종류>/<모드>.json` 패치가 기록되며 바꾼 값만 들어갑니다. 소유자의 파일은 바이트 단위로 그대로 남습니다. **Reset to inherited**는 패치를 지우고, 필드 옆 **↺**는 그 필드에 대한 여러분의 값을 지웁니다.

## 명령 팔레트

왼쪽에서 행동이나 명령을 고르고("피해", "아이템", "check" 같은 말로 검색), 양식을 채우면 그 아래에 줄이 나타납니다:

![Change a stat for good을 고른 팔레트: "모드 stats_base가 설명함", Stat Strength · str, Value 2, Who 비어 있음, 줄 "action stat.add stat=str value=2", "줄에 문제 없음".](assets/palette-line.jpg)

줄은 입력하는 동안 Validator와 같은 규칙으로 검사됩니다. 스탯 이름을 잘못 적으면:

![스탯을 strenght로 한 같은 양식: 줄 "action stat.add stat=strenght value=2", 오류 "'stat.add': 'stat'이 가리키는 스탯 'strenght'을 불러온 어느 모드도 정의하지 않습니다".](assets/palette-misspelt.jpg)

**넣기**는 시나리오 에디터에서 연 시나리오의 커서 아래에 줄을 넣고, **복사**는 클립보드에 넣습니다. RPG Plus의 예언자 시나리오에 넣은 뒤:

![시나리오 에디터에서 변경됨으로 표시된 seer.scenario(seer *): 2번째 줄이 이제 "action stat.add stat=str value=2"이며, 예언자의 인사와 행운 판정 사이에 있습니다.](assets/scenario-inserted.jpg)

`flag` 필드에는 **self.** 체크 상자가 있습니다. 켜면 그 줄은 실행 중인 대화의 오브젝트 플래그를 씁니다(`flag self.picked 1`). 이름이 없는 `self.`만 있으면 줄 아래에 오류로 표시됩니다. **삽입**은 이 때문에 멈추지 않으니, 먼저 이름을 고치세요.

작성자가 시나리오 에디터에서 **저장**을 누르기 전까지 디스크에는 아무것도 기록되지 않습니다. 시나리오 텍스트가 언제나 진실의 원천이며, 팔레트는 그것을 쓰도록 도울 뿐입니다.

## 문제가 생겼을 때

1. **종류나 행동이 없습니다.** 불러온 어떤 모드도 스키마로 설명하지 않았습니다. Schemas 페이지에 추가하는 방법이 있습니다.
2. **모드 목록이 비어 있습니다.** 쓸 수 있는 모드가 없습니다(폴더로 설치된 모드에만 쓸 수 있고, 압축된 모드에는 쓸 수 없습니다).
3. **넣기에서 시나리오가 열려 있지 않다고 합니다.** 먼저 시나리오 에디터에서 열거나 복사를 쓰세요.
4. **필드가 평범한 텍스트 상자입니다.** 스키마상 유형이 text이거나 알 수 없습니다.

## 추가 필드

`quests_base`, `skills_base`, `turn_battle_base`가 퀘스트, 스킬, 상태를 정의합니다. 퀘스트 단계는 번호와 언어별 텍스트로 편집하며 유형과 액션은 선택기입니다. 플래그 필드의 **self.**로 `flag self.picked 1`을 만듭니다. 이름 없는 `self.`는 잘못된 값이므로 삽입 전에 고치세요.
