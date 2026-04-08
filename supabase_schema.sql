-- ============================================
-- DM League Manager - Supabase Database Schema
-- ============================================
-- Supabaseのダッシュボード > SQL Editor で実行してください

-- プレイヤーテーブル
create table players (
  id serial primary key,
  name text not null,
  created_at timestamptz default now()
);

-- シーズンテーブル
create table seasons (
  id serial primary key,
  name text not null,
  status text not null default 'active', -- active | finished
  created_at timestamptz default now()
);

-- シーズン参加テーブル（プレイヤーのシーズンごとの状態）
create table season_players (
  id serial primary key,
  season_id int references seasons(id) on delete cascade,
  player_id int references players(id) on delete cascade,
  remaining_decks int not null default 6,
  win_count int not null default 0,
  lose_count int not null default 0,
  rank int,              -- null = まだ脱落していない
  total_points int not null default 0,
  created_at timestamptz default now(),
  unique(season_id, player_id)
);

-- デッキテーブル
create table decks (
  id serial primary key,
  season_player_id int references season_players(id) on delete cascade,
  name text not null,
  status text not null default 'active', -- active | lost
  created_at timestamptz default now()
);

-- 試合テーブル
create table matches (
  id serial primary key,
  season_id int references seasons(id) on delete cascade,
  winner_player_id int references players(id),
  loser_player_id int references players(id),
  lost_deck_id int references decks(id),
  played_at timestamptz default now()
);

-- ============================================
-- Row Level Security (RLS) 設定
-- 全員が読める、書き込みも全員可能（認証なし運用）
-- ============================================

alter table players enable row level security;
alter table seasons enable row level security;
alter table season_players enable row level security;
alter table decks enable row level security;
alter table matches enable row level security;

-- 全員読み取り可能
create policy "Anyone can read players" on players for select using (true);
create policy "Anyone can read seasons" on seasons for select using (true);
create policy "Anyone can read season_players" on season_players for select using (true);
create policy "Anyone can read decks" on decks for select using (true);
create policy "Anyone can read matches" on matches for select using (true);

-- 全員書き込み可能（認証不要運用）
create policy "Anyone can insert players" on players for insert with check (true);
create policy "Anyone can insert seasons" on seasons for insert with check (true);
create policy "Anyone can insert season_players" on season_players for insert with check (true);
create policy "Anyone can insert decks" on decks for insert with check (true);
create policy "Anyone can insert matches" on matches for insert with check (true);

-- 全員更新可能
create policy "Anyone can update season_players" on season_players for update using (true);
create policy "Anyone can update decks" on decks for update using (true);
create policy "Anyone can update seasons" on seasons for update using (true);

-- ============================================
-- テストデータ（任意）
-- ============================================

-- insert into players (name) values ('鈴木'), ('佐藤'), ('田中');
-- insert into seasons (name) values ('2025年春シーズン');
