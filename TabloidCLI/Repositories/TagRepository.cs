﻿using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using TabloidCLI.Models;
using TabloidCLI.Repositories;
using TabloidCLI.UserInterfaceManagers;

namespace TabloidCLI
{
    public class TagRepository : DatabaseConnector, IRepository<Tag>
    {
        public TagRepository(string connectionString) : base(connectionString) { }

        public List<Tag> GetAll()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT id, Name FROM Tag";
                    List<Tag> tags = new List<Tag>();

                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        Tag tag = new Tag()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                        };
                        tags.Add(tag);
                    }

                    reader.Close();

                    return tags;
                }
            }
        }

        public Tag Get(int id)
        {
            throw new NotImplementedException();
        }

        public List<Tag> GetTagsByPost(int postId)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT Tag.Name
                                        FROM Tag LEFT JOIN PostTag ON PostTag.TagId = Tag.Id
                                        WHERE PostTag.PostId = @postId";
                    cmd.Parameters.AddWithValue("@postId", postId);
                    List<Tag> tags = new List<Tag>();
                    

                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
         
                        Tag tag = new Tag()
                        {
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                        };  
                        tags.Add(tag);
                    };    
                    reader.Close();

                    return tags;
                }

            }
        }

        public void Insert(Tag tag)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Tag (Name)
                                                     VALUES (@name)";
                    cmd.Parameters.AddWithValue("@name", tag.Name);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void Update(Tag tag)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"UPDATE Tag 
                                           SET Name = @name
                                         WHERE id = @id";

                    cmd.Parameters.AddWithValue("@name", tag.Name);
                    cmd.Parameters.AddWithValue("@id", tag.Id);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void Delete(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"DELETE FROM Tag WHERE id = @id";
                    cmd.Parameters.AddWithValue("@id", id);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public SearchResults<Author> SearchAuthors(string tagName)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT a.id,
                                               a.FirstName,
                                               a.LastName,
                                               a.Bio
                                          FROM Author a
                                               LEFT JOIN AuthorTag at on a.Id = at.AuthorId
                                               LEFT JOIN Tag t on t.Id = at.TagId
                                         WHERE t.Name LIKE @name";
                    cmd.Parameters.AddWithValue("@name", $"%{tagName}%");
                    SqlDataReader reader = cmd.ExecuteReader();

                    SearchResults<Author> results = new SearchResults<Author>();
                    while (reader.Read())
                    {
                        Author author = new Author()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            Bio = reader.GetString(reader.GetOrdinal("Bio")),
                        };
                        results.Add(author);
                    }

                    reader.Close();

                    return results;
                }
            }
        }

        public SearchResults<Blog> SearchBlogs(string tagName)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT a.id,
                                               a.Title,
                                               a.Url
                                          FROM Blog a
                                               LEFT JOIN BlogTag at on a.Id = at.BlogId
                                               LEFT JOIN Tag t on t.Id = at.TagId
                                         WHERE t.Name LIKE @name";

                    cmd.Parameters.AddWithValue("@name", $"%{tagName}%");
                    SqlDataReader reader = cmd.ExecuteReader();

                    SearchResults<Blog> results = new SearchResults<Blog>();
                    while (reader.Read())
                    {
                        Blog blog = new Blog()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Title = reader.GetString(reader.GetOrdinal("Title")),
                            Url = reader.GetString(reader.GetOrdinal("Url")),
                        };
                        results.Add(blog);
                    }

                    reader.Close();

                    return results;
                }
            }
        }


        public SearchResults<Post> SearchPosts(string tagName)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT a.id,
                                               a.Title,
                                               a.Url,
                                               a.PublishDateTime,
                                               a.AuthorId,
                                               a.BlogId
                                          FROM Post a
                                               LEFT JOIN PostTag at on a.Id = at.PostId
                                               LEFT JOIN Tag t on t.Id = at.TagId
                                         WHERE t.Name LIKE @name";

                    cmd.Parameters.AddWithValue("@name", $"%{tagName}%");
                    SqlDataReader reader = cmd.ExecuteReader();

                    SearchResults<Post> results = new SearchResults<Post>();
                    while (reader.Read())
                    {
                        Post post = new Post()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Title = reader.GetString(reader.GetOrdinal("Title")),
                            Url = reader.GetString(reader.GetOrdinal("Url")),
                            PublishDateTime = reader.GetDateTime(reader.GetOrdinal("PublishDateTime")),

                        };
                        results.Add(post);
                    }

                    reader.Close();

                    return results;
                }
            }
        }

        public SearchResults<object> SearchAll(string tagName)
        {

            // Search for authors
            SearchResults<Author> authorResults = SearchAuthors(tagName);

            // Search for blogs
            SearchResults<Blog> blogResults = SearchBlogs(tagName);

            // Search for posts
            SearchResults<Post> postResults = SearchPosts(tagName);

            // Combine the results into a single SearchResults<t> t
            SearchResults<object> newResults = new SearchResults<object>();

            foreach (var result in authorResults._results)
            {
                newResults.Add(result);
            }

            foreach (var result in blogResults._results)
            {
                newResults.Add(result);
            }

            foreach (var result in postResults._results)
            {
                newResults.Add(result);

            }

            return newResults;
        }

    }
}
