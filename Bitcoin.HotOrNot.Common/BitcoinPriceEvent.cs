using System;

namespace Bitcoin.HotOrNot.Common {
    public class BitcoinPriceEvent {

        //--- Properties ---
        public double Price { get; set; }
    }

    public class BitcoinVoteEvent {

        //--- Properties ---
        public string VoterId { get; set; }
        public bool Vote { get; set; }
    }
}
