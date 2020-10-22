namespace Bitcoin.HotOrNot.Common {

    public sealed class BitcoinVoteEvent {

        //--- Properties ---
        public string VoterId { get; set; }
        public bool Vote { get; set; }
    }
}
