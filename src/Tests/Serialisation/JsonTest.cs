using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using VVVV.Packs.Messaging.Nodes;

namespace VVVV.Packs.Messaging.Tests
{
    [TestClass]
    public class JsonTest
    {
        VVVVProfile profile = new VVVVProfile();

        #region basics
        [TestMethod]
        public void Empty()
        {
            var json = @"{ }";
            var message = JsonConvert.DeserializeObject<Message>(json);

            Assert.IsTrue(message.IsEmpty);
            Assert.AreEqual("vvvv", message.Topic);
        }

        [TestMethod]
        public void Topic()
        {
            var json = @"{'Topic':'Test'}";
            var message = JsonConvert.DeserializeObject<Message>(json);

            Assert.IsTrue(message.IsEmpty);
            Assert.AreEqual("Test", message.Topic);
        }
        #endregion basics

        #region bin

        [TestMethod]
        public void Single()
        {
            var json = @"{'Field':'Test'}";
            var message = JsonConvert.DeserializeObject<Message>(json);

            Assert.IsFalse(message.IsEmpty);
            Assert.AreEqual("Test", message["Field"].First);
            Assert.AreEqual(1, message["Field"].Count);

        }

        [TestMethod]
        public void Multiple()
        {
            var json = @"{'Field':['Test', 'Foo', 'Bar']}";
            var message = JsonConvert.DeserializeObject<Message>(json);

            Assert.IsFalse(message.IsEmpty);

            var bin = BinFactory.New("Test", "Foo", "Bar");
            Assert.AreEqual(bin, message["Field"]);
        }


        [TestMethod]
        public void EmptyArray()
        {
            var json = @"{'Field':[]}";
            var message = JsonConvert.DeserializeObject<Message>(json);

            Assert.IsTrue(message.IsEmpty);
        }

        [TestMethod]
        public void Nested()
        {
            var json = @"{'Field':{} }";
            var message = JsonConvert.DeserializeObject<Message>(json);

            Assert.IsFalse(message.IsEmpty);
            Assert.AreEqual(1, message["Field"].Count);
            Assert.IsTrue(((Message)(message["Field"].First)).IsEmpty);
        }

        [TestMethod]
        public void IMDB()
        {
            #region http://sg.media-imdb.com/suggests/h/hello.json
            var json = @"{
              'v': 1,
              'q': 'hello',
              'd': [
                {
                  'y': 2015,
                  'l': 'Hello, My Name Is Doris',
                  'q': 'feature',
                  'id': 'tt3766394',
                  's': 'Sally Field, Max Greenfield',
                  'i': [
                    'http:\/\/ia.media-imdb.com\/images\/M\/MV5BMTg0NTM3MTI1MF5BMl5BanBnXkFtZTgwMTAzNTAzNzE@._V1_.jpg',
                    486,
                    720
                  ]
                },
                {
                  'y': 2011,
                  'l': 'Hell on Wheels',
                  'q': 'TV series',
                  'id': 'tt1699748',
                  's': 'Anson Mount, Colm Meaney',
                  'i': [
                    'http:\/\/ia.media-imdb.com\/images\/M\/MV5BMjM5ODQ5Nzc3OF5BMl5BanBnXkFtZTgwOTQzMzM4NjE@._V1_.jpg',
                    509,
                    755
                  ]
                },
                {
                  'y': 2016,
                  'l': 'Hell or High Water',
                  'q': 'feature',
                  'id': 'tt2582782',
                  's': 'Chris Pine, Ben Foster',
                  'i': [
                    'http:\/\/ia.media-imdb.com\/images\/M\/MV5BMzAxMWJiZDUtMGRjZi00ODZmLWFiZjktMzViODkwZmQyNTE4XkEyXkFqcGdeQXVyNjUwNzk3NDc@._V1_.jpg',
                    528,
                    912
                  ]
                },
                {
                  'y': 2013,
                  'l': 'Hello Ladies',
                  'q': 'TV series',
                  'id': 'tt2378794',
                  's': 'Stephen Merchant, Christine Woods',
                  'i': [
                    'http:\/\/ia.media-imdb.com\/images\/M\/MV5BNjYxMjI3MzY3NF5BMl5BanBnXkFtZTgwMTgyNzg3MDE@._V1_.jpg',
                    1423,
                    2048
                  ]
                },
                {
                  'y': 1969,
                  'l': 'Hello, Dolly!',
                  'q': 'feature',
                  'id': 'tt0064418',
                  's': 'Barbra Streisand, Walter Matthau',
                  'i': [
                    'http:\/\/ia.media-imdb.com\/images\/M\/MV5BMTE5OTc3MDQxNl5BMl5BanBnXkFtZTcwNTg2MTAwMQ@@._V1_.jpg',
                    336,
                    475
                  ]
                },
                {
                  'y': 2015,
                  'l': 'Hello, It\'s Me',
                  'q': 'TV movie',
                  'id': 'tt5063384',
                  's': 'Kellie Martin, Kavan Smith',
                  'i': [
                    'http:\/\/ia.media-imdb.com\/images\/M\/MV5BYjNlYjlmYzUtZWIzMC00NjI5LWJlNjAtODQyZTc1YmQwMzc2XkEyXkFqcGdeQXVyNTE0NTM0Ng@@._V1_.jpg',
                    449,
                    674
                  ]
                },
                {
                  'y': 2012,
                  'l': 'Hello I Must Be Going',
                  'q': 'feature',
                  'id': 'tt2063666',
                  's': 'Melanie Lynskey, Christopher Abbott',
                  'i': [
                    'http:\/\/ia.media-imdb.com\/images\/M\/MV5BMzkzMDc0Nzg5OF5BMl5BanBnXkFtZTcwMDU0MzAyOA@@._V1_.jpg',
                    450,
                    657
                  ]
                },
                {
                  'y': 2013,
                  'l': 'Hello Carter',
                  'q': 'feature',
                  'id': 'tt2636488',
                  's': 'Charlie Cox, Annabelle Wallis',
                  'i': [
                    'http:\/\/ia.media-imdb.com\/images\/M\/MV5BMTcxMDAwNDUwM15BMl5BanBnXkFtZTgwMzgxODI1MzE@._V1_.jpg',
                    866,
                    1281
                  ]
                }
              ]
            }";
            #endregion
            var message = JsonConvert.DeserializeObject<Message>(json);

            Assert.IsFalse(message.IsEmpty);
            Assert.AreEqual(3, message.Fields.Count()); // v, q, d

            Assert.AreEqual(1, message["v"].First);
            Assert.AreEqual("hello", message["q"].First);

            var nestings = message["d"] as Bin<Message>;
            Assert.AreEqual(8, nestings.Count);

            Assert.AreEqual("Hello, My Name Is Doris", nestings.First["l"].First);
        }

        [TestMethod]
        public void Twitter()
        {
            #region https://dev.twitter.com/rest/reference/get/search/tweets
            var json = @"{
                'statuses': [
                  {
                      'coordinates': null,
                                    'favorited': false,
                                    'truncated': false,
                                    'created_at': 'Mon Sep 24 03:35:21 +0000 2012',
                                    'id_str': '250075927172759552',
                                    'entities': {
                        'urls': [
              

                                      ],
                        'hashtags': [
                          {
                            'text': 'freebandnames',
                            'indices': [
                              20,
                              34
                            ]
                    }
                        ],
                        'user_mentions': [
                        ]
                },
                      'in_reply_to_user_id_str': null,
                      'contributors': null,
                      'text': 'Aggressive Ponytail #freebandnames',
                      'metadata': {
                        'iso_language_code': 'en',
                        'result_type': 'recent'
                      },
                      'retweet_count': 0,
                      'in_reply_to_status_id_str': null,
                      'id': 250075927172759552,
                      'geo': null,
                      'retweeted': false,
                      'in_reply_to_user_id': null,
                      'place': null,
                      'user': {
                        'profile_sidebar_fill_color': 'DDEEF6',
                        'profile_sidebar_border_color': 'C0DEED',
                        'profile_background_tile': false,
                        'name': 'Sean Cummings',
                        'profile_image_url': 'http://a0.twimg.com/profile_images/2359746665/1v6zfgqo8g0d3mk7ii5s_normal.jpeg',
                        'created_at': 'Mon Apr 26 06:01:55 +0000 2010',
                        'location': 'LA, CA',
                        'follow_request_sent': null,
                        'profile_link_color': '0084B4',
                        'is_translator': false,
                        'id_str': '137238150',
                        'entities': {
                          'url': {
                            'urls': [
                              {
                                'expanded_url': null,
                                'url': '',
                                'indices': [
                                  0,
                                  0
                                ]
                              }
                            ]
                          },
                          'description': {
                            'urls': [


                            ]
                          }
                        },
                        'default_profile': true,
                        'contributors_enabled': false,
                        'favourites_count': 0,
                        'url': null,
                        'profile_image_url_https': 'https://si0.twimg.com/profile_images/2359746665/1v6zfgqo8g0d3mk7ii5s_normal.jpeg',
                        'utc_offset': -28800,
                        'id': 137238150,
                        'profile_use_background_image': true,
                        'listed_count': 2,
                        'profile_text_color': '333333',
                        'lang': 'en',
                        'followers_count': 70,
                        'protected': false,
                        'notifications': null,
                        'profile_background_image_url_https': 'https://si0.twimg.com/images/themes/theme1/bg.png',
                        'profile_background_color': 'C0DEED',
                        'verified': false,
                        'geo_enabled': true,
                        'time_zone': 'Pacific Time (US & Canada)',
                        'description': 'Born 330 Live 310',
                        'default_profile_image': false,
                        'profile_background_image_url': 'http://a0.twimg.com/images/themes/theme1/bg.png',
                        'statuses_count': 579,
                        'friends_count': 110,
                        'following': null,
                        'show_all_inline_media': false,
                        'screen_name': 'sean_cummings'
                      },
                      'in_reply_to_screen_name': null,
                      'source': '<a href=\'//itunes.apple.com/us/app/twitter/id409789998?mt=12%5C%22\' rel=\'nofollow\'>Twitter for Mac</a>',
                      'in_reply_to_status_id': null
                    },
                    {
                      'coordinates': null,
                      'favorited': false,
                      'truncated': false,
                      'created_at': 'Fri Sep 21 23:40:54 +0000 2012',
                      'id_str': '249292149810667520',
                      'entities': {
                        'urls': [


                        ],
                        'hashtags': [
                          {
                            'text': 'FreeBandNames',
                            'indices': [
                              20,
                              34
                            ]
                          }
                        ],
                        'user_mentions': [


                        ]
                      },
                      'in_reply_to_user_id_str': null,
                      'contributors': null,
                      'text': 'Thee Namaste Nerdz. #FreeBandNames',
                      'metadata': {
                        'iso_language_code': 'pl',
                        'result_type': 'recent'
                      },
                      'retweet_count': 0,
                      'in_reply_to_status_id_str': null,
                      'id': 249292149810667520,
                      'geo': null,
                      'retweeted': false,
                      'in_reply_to_user_id': null,
                      'place': null,
                      'user': {
                        'profile_sidebar_fill_color': 'DDFFCC',
                        'profile_sidebar_border_color': 'BDDCAD',
                        'profile_background_tile': true,
                        'name': 'Chaz Martenstein',
                        'profile_image_url': 'http://a0.twimg.com/profile_images/447958234/Lichtenstein_normal.jpg',
                        'created_at': 'Tue Apr 07 19:05:07 +0000 2009',
                        'location': 'Durham, NC',
                        'follow_request_sent': null,
                        'profile_link_color': '0084B4',
                        'is_translator': false,
                        'id_str': '29516238',
                        'entities': {
                          'url': {
                            'urls': [
                              {
                                'expanded_url': null,
                                'url': 'http://bullcityrecords.com/wnng/',
                                'indices': [
                                  0,
                                  32
                                ]
                              }
                            ]
                          },
                          'description': {
                            'urls': [


                            ]
                          }
                        },
                        'default_profile': false,
                        'contributors_enabled': false,
                        'favourites_count': 8,
                        'url': 'http://bullcityrecords.com/wnng/',
                        'profile_image_url_https': 'https://si0.twimg.com/profile_images/447958234/Lichtenstein_normal.jpg',
                        'utc_offset': -18000,
                        'id': 29516238,
                        'profile_use_background_image': true,
                        'listed_count': 118,
                        'profile_text_color': '333333',
                        'lang': 'en',
                        'followers_count': 2052,
                        'protected': false,
                        'notifications': null,
                        'profile_background_image_url_https': 'https://si0.twimg.com/profile_background_images/9423277/background_tile.bmp',
                        'profile_background_color': '9AE4E8',
                        'verified': false,
                        'geo_enabled': false,
                        'time_zone': 'Eastern Time (US & Canada)',
                        'description': 'You will come to Durham, North Carolina. I will sell you some records then, here in Durham, North Carolina. Fun will happen.',
                        'default_profile_image': false,
                        'profile_background_image_url': 'http://a0.twimg.com/profile_background_images/9423277/background_tile.bmp',
                        'statuses_count': 7579,
                        'friends_count': 348,
                        'following': null,
                        'show_all_inline_media': true,
                        'screen_name': 'bullcityrecords'
                      },
                      'in_reply_to_screen_name': null,
                      'source': 'web',
                      'in_reply_to_status_id': null
                    },
                    {
                      'coordinates': null,
                      'favorited': false,
                      'truncated': false,
                      'created_at': 'Fri Sep 21 23:30:20 +0000 2012',
                      'id_str': '249289491129438208',
                      'entities': {
                        'urls': [


                        ],
                        'hashtags': [
                          {
                            'text': 'freebandnames',
                            'indices': [
                              29,
                              43
                            ]
                          }
                        ],
                        'user_mentions': [


                        ]
                      },
                      'in_reply_to_user_id_str': null,
                      'contributors': null,
                      'text': 'Mexican Heaven, Mexican Hell #freebandnames',
                      'metadata': {
                        'iso_language_code': 'en',
                        'result_type': 'recent'
                      },
                      'retweet_count': 0,
                      'in_reply_to_status_id_str': null,
                      'id': 249289491129438208,
                      'geo': null,
                      'retweeted': false,
                      'in_reply_to_user_id': null,
                      'place': null,
                      'user': {
                        'profile_sidebar_fill_color': '99CC33',
                        'profile_sidebar_border_color': '829D5E',
                        'profile_background_tile': false,
                        'name': 'Thomas John Wakeman',
                        'profile_image_url': 'http://a0.twimg.com/profile_images/2219333930/Froggystyle_normal.png',
                        'created_at': 'Tue Sep 01 21:21:35 +0000 2009',
                        'location': 'Kingston New York',
                        'follow_request_sent': null,
                        'profile_link_color': 'D02B55',
                        'is_translator': false,
                        'id_str': '70789458',
                        'entities': {
                          'url': {
                            'urls': [
                              {
                                'expanded_url': null,
                                'url': '',
                                'indices': [
                                  0,
                                  0
                                ]
                              }
                            ]
                          },
                          'description': {
                            'urls': [


                            ]
                          }
                        },
                        'default_profile': false,
                        'contributors_enabled': false,
                        'favourites_count': 19,
                        'url': null,
                        'profile_image_url_https': 'https://si0.twimg.com/profile_images/2219333930/Froggystyle_normal.png',
                        'utc_offset': -18000,
                        'id': 70789458,
                        'profile_use_background_image': true,
                        'listed_count': 1,
                        'profile_text_color': '3E4415',
                        'lang': 'en',
                        'followers_count': 63,
                        'protected': false,
                        'notifications': null,
                        'profile_background_image_url_https': 'https://si0.twimg.com/images/themes/theme5/bg.gif',
                        'profile_background_color': '352726',
                        'verified': false,
                        'geo_enabled': false,
                        'time_zone': 'Eastern Time (US & Canada)',
                        'description': 'Science Fiction Writer, sort of. Likes Superheroes, Mole People, Alt. Timelines.',
                        'default_profile_image': false,
                        'profile_background_image_url': 'http://a0.twimg.com/images/themes/theme5/bg.gif',
                        'statuses_count': 1048,
                        'friends_count': 63,
                        'following': null,
                        'show_all_inline_media': false,
                        'screen_name': 'MonkiesFist'
                      },
                      'in_reply_to_screen_name': null,
                      'source': 'web',
                      'in_reply_to_status_id': null
                    },
                    {
                      'coordinates': null,
                      'favorited': false,
                      'truncated': false,
                      'created_at': 'Fri Sep 21 22:51:18 +0000 2012',
                      'id_str': '249279667666817024',
                      'entities': {
                        'urls': [


                        ],
                        'hashtags': [
                          {
                            'text': 'freebandnames',
                            'indices': [
                              20,
                              34
                            ]
                          }
                        ],
                        'user_mentions': [


                        ]
                      },
                      'in_reply_to_user_id_str': null,
                      'contributors': null,
                      'text': 'The Foolish Mortals #freebandnames',
                      'metadata': {
                        'iso_language_code': 'en',
                        'result_type': 'recent'
                      },
                      'retweet_count': 0,
                      'in_reply_to_status_id_str': null,
                      'id': 249279667666817024,
                      'geo': null,
                      'retweeted': false,
                      'in_reply_to_user_id': null,
                      'place': null,
                      'user': {
                        'profile_sidebar_fill_color': 'BFAC83',
                        'profile_sidebar_border_color': '615A44',
                        'profile_background_tile': true,
                        'name': 'Marty Elmer',
                        'profile_image_url': 'http://a0.twimg.com/profile_images/1629790393/shrinker_2000_trans_normal.png',
                        'created_at': 'Mon May 04 00:05:00 +0000 2009',
                        'location': 'Wisconsin, USA',
                        'follow_request_sent': null,
                        'profile_link_color': '3B2A26',
                        'is_translator': false,
                        'id_str': '37539828',
                        'entities': {
                          'url': {
                            'urls': [
                              {
                                'expanded_url': null,
                                'url': 'http://www.omnitarian.me',
                                'indices': [
                                  0,
                                  24
                                ]
                              }
                            ]
                          },
                          'description': {
                            'urls': [


                            ]
                          }
                        },
                        'default_profile': false,
                        'contributors_enabled': false,
                        'favourites_count': 647,
                        'url': 'http://www.omnitarian.me',
                        'profile_image_url_https': 'https://si0.twimg.com/profile_images/1629790393/shrinker_2000_trans_normal.png',
                        'utc_offset': -21600,
                        'id': 37539828,
                        'profile_use_background_image': true,
                        'listed_count': 52,
                        'profile_text_color': '000000',
                        'lang': 'en',
                        'followers_count': 608,
                        'protected': false,
                        'notifications': null,
                        'profile_background_image_url_https': 'https://si0.twimg.com/profile_background_images/106455659/rect6056-9.png',
                        'profile_background_color': 'EEE3C4',
                        'verified': false,
                        'geo_enabled': false,
                        'time_zone': 'Central Time (US & Canada)',
                        'description': 'Cartoonist, Illustrator, and T-Shirt connoisseur',
                        'default_profile_image': false,
                        'profile_background_image_url': 'http://a0.twimg.com/profile_background_images/106455659/rect6056-9.png',
                        'statuses_count': 3575,
                        'friends_count': 249,
                        'following': null,
                        'show_all_inline_media': true,
                        'screen_name': 'Omnitarian'
                      },
                      'in_reply_to_screen_name': null,
                      'source': '<a href=\'//twitter.com/download/iphone%5C%22\' rel=\'nofollow\'>Twitter for iPhone</a>',
                      'in_reply_to_status_id': null
                    }
                  ],
                  'search_metadata': {
                    'max_id': 250126199840518145,
                    'since_id': 24012619984051000,
                    'refresh_url': '?since_id=250126199840518145&q=%23freebandnames&result_type=mixed&include_entities=1',
                    'next_results': '?max_id=249279667666817023&q=%23freebandnames&count=4&include_entities=1&result_type=mixed',
                    'count': 4,
                    'completed_in': 0.035,
                    'since_id_str': '24012619984051000',
                    'query': '%23freebandnames',
                    'max_id_str': '250126199840518145'
                  }
                }";
            #endregion 

            var message = JsonConvert.DeserializeObject<Message>(json);

            Assert.IsFalse(message.IsEmpty);

            var stati = message["statuses"] as Bin<Message>;
            Assert.AreEqual(4, stati.Count);
            Assert.AreEqual("Mon Sep 24 03:35:21 +0000 2012", stati.First["created_at"].First);
        }

        #endregion bin

        #region explicit
        [TestMethod]
        public void EmptyArrayExplicit()
        { 
            // use explicit typing in json.
            var json = @"{'Field<string>':[]}";
            var message = JsonConvert.DeserializeObject<Message>(json);

            Assert.IsFalse(message.IsEmpty);
            Assert.AreEqual(typeof(string), message["Field"].GetInnerType());

        }

        [TestMethod]
        public void ConvertExplicit()
        {
            var json = @"{'Field<int>':['1', '2', '3']}";
            var message = JsonConvert.DeserializeObject<Message>(json);

            Assert.IsFalse(message.IsEmpty);

            var bin = BinFactory.New(1, 2, 3);
            Assert.AreEqual(bin, message["Field"]);

        }
        #endregion explicit


    }
}
