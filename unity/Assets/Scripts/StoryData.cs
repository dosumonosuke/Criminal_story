using System;

namespace CriminalStory
{
    /// <summary>
    /// One subtitle line. Speaker may be empty (narration).
    /// Seconds is how long the line stays before the story moves on
    /// by itself (the player may also advance it manually).
    /// </summary>
    [Serializable]
    public struct StoryLine
    {
        public string Speaker;
        public string Text;
        public float Seconds;

        public StoryLine(string speaker, string text, float seconds)
        {
            Speaker = speaker;
            Text = text;
            Seconds = seconds;
        }
    }

    /// <summary>
    /// All story text used by the Unity foundation.
    ///
    /// Chapter 0 lines follow docs/SCRIPT_WUORNOS.md (序章) as written.
    /// Script dialogue is a dramatisation, NOT a record of real quotes;
    /// the only facts asserted are those covered by docs/RESEARCH.md
    /// (Encyclopaedia Britannica): born 1956-02-29 in Rochester, Michigan,
    /// as Aileen Carol Pittman; taken in by her maternal grandparents
    /// before the age of four and adopted with her brother Keith in
    /// March 1960. No crime methods and no graphic detail anywhere.
    /// </summary>
    public static class StoryData
    {
        public const string Chapter0Title =
            "第0章　生まれる ― 1956年2月29日／ミシガン州ロチェスター";
        public const string Chapter1Title =
            "第1章　祖父母の家 ― 1960年／ミシガン州トロイ";

        // ---- Chapter 0: the moment of birth (script: 序章) ----
        public static readonly StoryLine[] BirthLines =
        {
            new StoryLine("", "（……遠くで、赤ん坊の泣き声……）", 4.5f),
            new StoryLine("看護師", "……元気な泣き声ですよ", 4.5f),
            new StoryLine("医師", "女の子です。おめでとうございます", 5.0f),
            new StoryLine("看護師", "はい、よく泣いたね。……ほら、お母さんのところへ行こうね", 5.5f),
            new StoryLine("ダイアン（母親）", "……来たの、あんた", 4.0f),
            new StoryLine("ダイアン（母親）", "名前はね、決めてあるの。アイリーン。アイリーン・キャロル・ピットマン", 6.0f),
            new StoryLine("看護師", "2月29日生まれ。四年に一度しか誕生日が来ない子ですね", 5.5f),
            new StoryLine("ダイアン（母親）", "……特別でいいの。この子は、特別でいい", 5.0f),
            new StoryLine("", "父とは、一度も会うことがなかった", 5.5f),
            new StoryLine("", "こうして一人の女の子が生まれた。この子の人生を、これから一人称でたどっていく。", 6.0f),
        };

        // ---- Chapter 1 (town): talking to the grandmother ----
        // Built from the script's Chapter 1 situation (the children come
        // to live at the maternal grandparents' house; brother Keith is
        // there) and the facts in docs/RESEARCH.md. The day's details
        // are dramatisation, as the script itself prescribes.
        public static readonly StoryLine[] GrandmotherDialogue =
        {
            new StoryLine("祖母", "おかえり、アイリーン。……ここが、あなたの家よ", 5.0f),
            new StoryLine("", "1960年、4歳になる直前。母は家を出て、兄キースとアイリーンは母方の祖父母に引き取られた。同年3月、2人は正式に養子となった。", 7.0f),
            new StoryLine("キース（兄）", "リー、こっち。今日から、ここがうちだって", 5.0f),
            new StoryLine("祖母", "さあ、中に入りましょう。キースも待っているわ", 5.0f),
            new StoryLine("", "穏やかとは言えない日々が、この家で始まろうとしていた――。", 5.5f),
        };

        public const string TownObjective =
            "目的：金のマーカーの下にいる「祖母」に近づいて、E で話しかける";
        public const string TownObjectiveDone =
            "目的：達成しました（このまま町を見回せます）";

        public const string EndPanelTitle = "試作版（Unity 土台）はここまでです";
        public const string EndPanelBody =
            "第0章（生まれる場面）と、第1章の舞台であるミシガンの住宅街を歩き、" +
            "祖母と話すところまでを Unity で作り直した段階です。\n" +
            "今後はこの土台の上に、章ごとの場面と会話を足していきます。\n\n" +
            "事実の出典：Encyclopaedia Britannica「Aileen Wuornos」ほか" +
            "（詳しくは docs/RESEARCH.md）。セリフは脚本上の創作を含みます。";
    }
}
