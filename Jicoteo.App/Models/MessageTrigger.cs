﻿using System.Collections.Generic;

namespace Jicoteo.Models
{
    public class MessageTrigger
    {
        public List<string> _answers;
        public List<string> _reactions;
        public List<string> _triggers;

        public MessageTrigger(
            IEnumerable<string> triggers,
            IEnumerable<string> answers,
            IEnumerable<string> reactions)
        {
        }
    }
}