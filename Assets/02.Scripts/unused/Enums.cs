namespace AI_Enum
{
public class Enums
    {
        public enum DialogueType {Fixed, Narration, AiAppend, AiRewrite }
        public enum RoleType { Girl, Painter, Musician, Core }
        public enum Intent { Comfort, AskMemory, AskTruth, Silence, OffTopic }
        public enum MergeState { complete_merge, incomplete_merge }
        public enum PuzzleStability { High, Medium, Low }
        public enum FallbackType { Timeout, Unsafe, Empty, ParseError }
        public enum ConditionOperator { same, diff, over, under }
        public enum Logic { AND, OR }
        public enum Importance { Core, Major, Minor }
    }
}
