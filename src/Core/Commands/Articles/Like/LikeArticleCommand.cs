﻿using System;

namespace Core.Commands
{
    public class LikeArticleCommand : Request
    {
        public LikeArticleCommand(Guid articleId)
        {
            ArticleId = articleId;
        }

        public Guid ArticleId { get; set; }
    }
}