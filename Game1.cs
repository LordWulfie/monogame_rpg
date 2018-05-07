using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using MonoGame.Extended;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Graphics;

namespace rpg.Desktop
{
    enum Dir {
        Up,
        Down,
        Left,
        Right
    }

    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Texture2D player_Sprite;
        Texture2D playerDown;
        Texture2D playerUp;
        Texture2D playerLeft;
        Texture2D playerRight;

        Texture2D eyeEnemy_Sprite;
        Texture2D snakeEnemy_Sprite;

        Texture2D bush_Sprite;
        Texture2D tree_Sprite;

        Texture2D heart_Sprite;
        Texture2D bullet_Sprite;

        TiledMapRenderer mapRenderer;
        TiledMap myMap;

        Camera2D cam;

        Player player = new Player();


        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 680;

        }


        protected override void Initialize()
        {
            mapRenderer = new TiledMapRenderer(GraphicsDevice);
            cam = new Camera2D(GraphicsDevice);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);


            player_Sprite = Content.Load<Texture2D>("Player/player");
            playerUp = Content.Load<Texture2D>("Player/playerUp");
            playerDown = Content.Load<Texture2D>("Player/playerDown");
            playerLeft = Content.Load<Texture2D>("Player/playerLeft");
            playerRight = Content.Load<Texture2D>("Player/playerRight");

            eyeEnemy_Sprite = Content.Load<Texture2D>("Enemies/eyeEnemy");
            snakeEnemy_Sprite = Content.Load<Texture2D>("Enemies/snakeEnemy");

            bush_Sprite = Content.Load<Texture2D>("Objects/bush");
            tree_Sprite = Content.Load<Texture2D>("Objects/tree");

            heart_Sprite = Content.Load<Texture2D>("Misc/heart");
            bullet_Sprite = Content.Load<Texture2D>("Misc/bullet");

            player.animations[0] = new AnimatedSprite(playerUp, 1, 4);
            player.animations[1] = new AnimatedSprite(playerDown, 1, 4);
            player.animations[2] = new AnimatedSprite(playerLeft, 1, 4);
            player.animations[3] = new AnimatedSprite(playerRight, 1, 4);

            myMap = Content.Load<TiledMap>("Misc/gameMap");

            TiledMapObject[] allEnemies = myMap.GetLayer<TiledMapObjectLayer>("Enemies").Objects;
            foreach(var en in allEnemies)
            {
                string type;
                en.Properties.TryGetValue("Type", out type);
                if(type == "Snake")
                    Enemy.enemies.Add(new Snake(en.Position));
                else if(type == "Eye")
                    Enemy.enemies.Add(new Eye(en.Position));
            }

            TiledMapObject[] allObstacles = myMap.GetLayer<TiledMapObjectLayer>("Obstacles").Objects;
            foreach (var o in allObstacles)
            {
                string type;
                o.Properties.TryGetValue("Type", out type);
                if (type == "Bush")
                    Obstacle.obstacles.Add(new Bush(o.Position));
                else if (type == "Tree")
                    Obstacle.obstacles.Add(new Tree(o.Position));
            }
            //Enemy.enemies.Add(new Snake(new Vector2(400, 400)));
            //Enemy.enemies.Add(new Eye(new Vector2(300, 300)));

            //Obstacle.obstacles.Add(new Tree(new Vector2(500, 100)));
            //Obstacle.obstacles.Add(new Bush(new Vector2(800, 400)));
        }


        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }


        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            if (player.Health > 0)
                player.Update(gameTime);

            cam.LookAt(player.Position);

            foreach (Projectile proj in Projectile.projectiles)
            {
                proj.Update(gameTime);
            }
            foreach (Enemy en in Enemy.enemies)
            {
                en.Update(gameTime,player.Position);
            }

            foreach (Projectile proj in Projectile.projectiles)
            {
                foreach (Enemy en in Enemy.enemies)
                {
                    int sum = proj.Radius + en.Radius;
                    if(Vector2.Distance(proj.Position,en.Position) < sum)
                    {
                        proj.Collided = true;
                        en.Health -= 1;
                    }
                }
                if(Obstacle.didCollide(proj.Position,proj.Radius))
                {
                    proj.Collided = true;
                }
            }

            foreach (Enemy en in Enemy.enemies)
            {
                int sum = player.Radius + en.Radius;
                if (Vector2.Distance(player.Position, en.Position) < sum && player.HealthTimer <= 0)
                {
                    player.Health -= 1;
                    player.HealthTimer = 1.5f;
                }
            }

            Projectile.projectiles.RemoveAll(p => p.Collided);
            Enemy.enemies.RemoveAll(e => e.Health <= 0);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.ForestGreen);

            mapRenderer.Draw(myMap,cam.GetViewMatrix());

            spriteBatch.Begin(transformMatrix: cam.GetViewMatrix());

            if (player.Health > 0)
                player.anim.Draw(spriteBatch, new Vector2(player.Position.X - 48, player.Position.Y - 48));
            
            foreach (Obstacle o in Obstacle.obstacles)
            {
                Texture2D spriteToDraw;

                if (o.GetType() == typeof(Tree))
                    spriteToDraw = tree_Sprite;
                else
                    spriteToDraw = bush_Sprite;
                
                spriteBatch.Draw(spriteToDraw, o.Position, Color.White);
            }

            foreach (Projectile proj in Projectile.projectiles)
            {
                spriteBatch.Draw(bullet_Sprite, new Vector2(proj.Position.X - proj.Radius, proj.Position.Y - proj.Radius), Color.White);
            }

            foreach (Enemy en in Enemy.enemies)
            {
                Texture2D spriteToDraw;
                int rad;

                if(en.GetType() == typeof(Snake))
                {
                    spriteToDraw = snakeEnemy_Sprite;
                    rad = 50;
                } 
                else 
                {
                    spriteToDraw = eyeEnemy_Sprite;
                    rad = 73;
                }
                spriteBatch.Draw(spriteToDraw,new Vector2(en.Position.X-rad,en.Position.Y-rad),Color.White);

            }

            spriteBatch.End();

            spriteBatch.Begin();
            for (int i = 0; i < player.Health; i++)
            {
                spriteBatch.Draw(heart_Sprite, new Vector2(3+  i * 63, 3), Color.White);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
