BEGIN;

TRUNCATE "OrderItems", "Orders", "CartItems", "Carts",
         "StoreStocks", "Tokens", "Products", "Stores", "Users"
    RESTART IDENTITY CASCADE;

INSERT INTO "Users" ("Id", "Email", "Username", "Password", "Admin", "TokenVersion") VALUES
    ('aaaaaaaa-0000-4000-8000-000000000001', 'admin@vinylium.test',    'admin',
     '$2a$11$8pdej3eX9sr6p7mnMtXyMuwwkBtWV6aEZ/FQln/qxjH9UmWfQ3bRC', true,  0),
    ('aaaaaaaa-0000-4000-8000-000000000002', 'shopper@vinylium.test',  'shopper',
     '$2a$11$8pdej3eX9sr6p7mnMtXyMuwwkBtWV6aEZ/FQln/qxjH9UmWfQ3bRC', false, 0),
    ('aaaaaaaa-0000-4000-8000-000000000003', 'deleteme@vinylium.test', 'deleteme',
     '$2a$11$8pdej3eX9sr6p7mnMtXyMuwwkBtWV6aEZ/FQln/qxjH9UmWfQ3bRC', false, 0);

INSERT INTO "Stores" ("Id", "Name", "Address", "City", "ContactNumber", "OpeningTime", "ClosingTime", "IsWarehouse") VALUES
    ('11111111-1111-4111-8111-111111111111', 'Vinylium Centar',    'Gramofonska 7', 'Zvukovac', '+381601234567', '09:00:00', '21:00:00', false),
    ('22222222-2222-4222-8222-222222222222', 'Vinylium Duvaniste', 'Vinilska 21',   'Zvukovac', '+381601234568', '10:00:00', '20:00:00', false),
    ('33333333-3333-4333-8333-333333333333', 'Vinylium Warehouse', 'Skladisna 3',   'Tonograd', '+381601234569', '08:00:00', '16:00:00', true);

INSERT INTO "Products" ("Barcode", "CatalogNumber", "Name", "Artist", "ImageUrl", "Price", "Type", "Runtime", "ReleaseDate", "InStock", "Tracklist") VALUES
    ('1000000000001', 'CL-1355', 'Kind of Blue', 'Miles Davis',
     'data:image/gif;base64,R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7',
     2500, 0, '45:44', '1959', true,
     '[{"Title": "So What", "Runtime": "09:22"}, {"Title": "Freddie Freeloader", "Runtime": "09:46"}, {"Title": "Blue in Green", "Runtime": "05:37"}]'),

    ('1000000000002', 'SHVL-804', 'The Dark Side of the Moon', 'Pink Floyd',
     'data:image/gif;base64,R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7',
     3200, 0, '42:49', '1973', true,
     '[{"Title": "Speak to Me", "Runtime": "01:30"}, {"Title": "Breathe", "Runtime": "02:43"}, {"Title": "Time", "Runtime": "06:53"}]'),

    ('1000000000003', 'DGCC-24425', 'Nevermind', 'Nirvana',
     'data:image/gif;base64,R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7',
     900, 1, '42:38', '1991', true,
     '[{"Title": "Smells Like Teen Spirit", "Runtime": "05:01"}, {"Title": "In Bloom", "Runtime": "04:14"}]'),

    ('1000000000004', 'NODATA-01', 'OK Computer', 'Radiohead',
     'data:image/gif;base64,R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7',
     1500, 2, '53:21', '1997', true,
     '[{"Title": "Airbag", "Runtime": "04:44"}, {"Title": "Paranoid Android", "Runtime": "06:23"}]'),

    ('1000000000005', 'FACT-10', 'Unknown Pleasures', 'Joy Division',
     'data:image/gif;base64,R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7',
     2800, 0, '39:21', '1979', false,
     '[{"Title": "Disorder", "Runtime": "03:29"}, {"Title": "Day of the Lords", "Runtime": "04:48"}]');

INSERT INTO "StoreStocks" ("Id", "StoreId", "ProductBarcode", "Quantity")
SELECT gen_random_uuid(), s."Id", v."Barcode", v."Quantity"
FROM "Stores" s
JOIN (VALUES
    ('Vinylium Centar',    '1000000000001', 10),
    ('Vinylium Duvaniste', '1000000000001',  5),
    ('Vinylium Warehouse', '1000000000001', 50),

    ('Vinylium Centar',    '1000000000002',  8),
    ('Vinylium Duvaniste', '1000000000002',  0),
    ('Vinylium Warehouse', '1000000000002', 40),

    ('Vinylium Centar',    '1000000000003',  3),
    ('Vinylium Duvaniste', '1000000000003',  3),
    ('Vinylium Warehouse', '1000000000003', 20),

    ('Vinylium Centar',    '1000000000004',  1),
    ('Vinylium Duvaniste', '1000000000004',  0),
    ('Vinylium Warehouse', '1000000000004',  0),

    ('Vinylium Centar',    '1000000000005',  0),
    ('Vinylium Duvaniste', '1000000000005',  0),
    ('Vinylium Warehouse', '1000000000005',  0)
) AS v("StoreName", "Barcode", "Quantity") ON v."StoreName" = s."Name";

COMMIT;
