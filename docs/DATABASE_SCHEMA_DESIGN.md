# 🗄️ ChronoPos Database Schema Design

## 📊 Schema Overview

This document outlines the complete database schema for ChronoPos - a comprehensive Point of Sale system with multi-language support, multi-location capabilities, and advanced business features.

**Database Engine**: MySQL/MariaDB  
**Total Tables**: 80+ tables  
**Architecture**: Multi-tenant, Multi-language, Enterprise-grade

---

## 🏗️ Schema Architecture Layers

### **Core System Architecture**
```
┌─────────────────────────────────────────┐
│           Presentation Layer            │ ← WPF Desktop Application
├─────────────────────────────────────────┤
│           Business Logic Layer          │ ← Application Services
├─────────────────────────────────────────┤
│              Data Layer                 │ ← Repository Pattern
├─────────────────────────────────────────┤
│           Database Schema               │ ← MySQL Database
│  ┌─────────────────────────────────────┐ │
│  │ System & Administration Module      │ │
│  │ Multi-language Support Module      │ │
│  │ User Management & Security Module  │ │
│  │ Company & Shop Management Module   │ │
│  │ Product & Inventory Module         │ │
│  │ Customer & Supplier Module         │ │
│  │ Transaction & Payment Module       │ │
│  │ Restaurant & Reservation Module    │ │
│  │ Reporting & Analytics Module       │ │
│  └─────────────────────────────────────┘ │
└─────────────────────────────────────────┘
```

---

## 📋 Database Schema Tables

### 🔧 System & Administration

#### activity_logs
Tracks all system activities for auditing and debugging purposes.
```sql
CREATE TABLE activity_logs (
  log_id        bigint PRIMARY KEY AUTO_INCREMENT,
  timestamp     datetime DEFAULT CURRENT_TIMESTAMP,
  log_level     varchar(20),                          -- INFO, WARNING, ERROR, DEBUG
  user_id       bigint,                               -- FK to users table
  module        varchar(50),                          -- Transaction, Inventory, Login, etc.
  action        varchar(100),                         -- Short action description
  description   text,                                 -- Detailed log info
  ip_address    varchar(45),                          -- IPv4 or IPv6
  device_info   varchar(255),                         -- POS terminal, OS, browser
  status        varchar(20) DEFAULT 'SUCCESS',        -- SUCCESS, FAILED
  reference_id  bigint,                               -- Related record ID (e.g., invoice_id)
  old_values    json,                                 -- Data before change
  new_values    json                                  -- Data after change
);
```

#### language
Stores supported languages for multilingual support.
```sql
CREATE TABLE language (
  id int PRIMARY KEY,
  language_name varchar(255),
  language_code varchar(255),
  is_rtl boolean,
  status varchar(255),
  created_by varchar(255),
  created_at timestamp,
  updated_by varchar(255),
  updated_at timestamp,
  deleted_at timestamp,
  deleted_by varchar(255)
);
```

#### language_keyword
Contains translation keys for multilingual labels.
```sql
CREATE TABLE language_keyword (
  id int PRIMARY KEY AUTO_INCREMENT,
  key varchar(100) UNIQUE,                            -- e.g., "home.title", "button.save"
  description text                                    -- Optional: explain where/why it's used
);
```

#### label_translation
Stores translated text for multilingual support.
```sql
CREATE TABLE label_translation (
  id int PRIMARY KEY,
  language_id int UNIQUE,
  translation_key varchar(255),
  value varchar(255),
  status varchar(255),
  created_by varchar(255),
  created_at timestamp,
  updated_by varchar(255),
  updated_at timestamp,
  deleted_at timestamp,
  deleted_by varchar(255)
);
```

#### industry_type
Defines different industry types for business categorization.
```sql
CREATE TABLE industry_type (
  id int PRIMARY KEY AUTO_INCREMENT,
  name varchar(100) UNIQUE,                           -- Industry name (e.g., Retail)
  description text,
  status bool,
  created_at timestamp,
  updated_by varchar(255),
  updated_at timestamp,
  deleted_at timestamp
);
```

#### industry_type_access
Defines access permissions for different industry types.
```sql
CREATE TABLE industry_type_access (
  id int PRIMARY KEY AUTO_INCREMENT,
  industry_type_id int UNIQUE,
  dashboard_access boolean DEFAULT false,
  reports_access boolean DEFAULT false,
  client_office_access boolean DEFAULT false,
  transactions_access boolean DEFAULT false,
  reservation_access boolean DEFAULT false,
  notifications_access boolean DEFAULT false,
  created_at timestamp DEFAULT NOW()
);
```

#### owner
Stores owner information for the POS system.
```sql
CREATE TABLE owner (
  id int PRIMARY KEY AUTO_INCREMENT,
  name varchar(100),
  email varchar(100) UNIQUE,
  password varchar(255),
  created_at timestamp DEFAULT NOW()
);
```

#### plan
Defines subscription plans for the POS system.
```sql
CREATE TABLE plan (
  id int PRIMARY KEY AUTO_INCREMENT,
  name varchar(100),                                  -- Basic, Standard, Pro
  description text,
  price decimal(10,2),
  billing_cycle enum('monthly', 'yearly') DEFAULT 'monthly',
  max_users int,
  max_outlets int,
  max_products int,
  max_invoices int,
  features text,                                      -- JSON or comma-separated
  is_active boolean DEFAULT true,
  priority int,
  created_at timestamp DEFAULT NOW()
);
```

#### company_settings
Stores configuration settings for companies using the POS.
```sql
CREATE TABLE company_settings (
  id                           int PRIMARY KEY AUTO_INCREMENT,
  company_id                   int,   
  currency_id                  int NOT NULL,          -- FK → currencies
  stock_value_id               int,        
  primary_color                varchar(20),           -- Hex or color code
  secondary_color              varchar(20),           -- Hex or color code
  client_backup_frequency      varchar(50),           -- e.g., Daily, Weekly
  atmacss_backup_frequency     varchar(50),           -- Frequency for Atmacss backups
  refund_type                  varchar(50),           -- e.g., Full, Partial
  period_of_validity           int,                   -- Days for validity
  allow_return_cash            boolean DEFAULT false,
  allow_credit_note            boolean DEFAULT false,
  allow_exchange_transaction   boolean DEFAULT false,
  has_sku_format               boolean DEFAULT false,
  has_invoice_format           boolean DEFAULT false,
  company_subscription_type    varchar(50),           -- e.g., Trial, Premium
  invoice_default_language_id  int,                   -- FK → languages
  number_of_users              int DEFAULT 1,
  invoice_printers             int,                   -- Count of printers
  barcode_scanners             int,                   -- Count of scanners
  normal_printer               int,                   -- Count of report printers
  barcode_printer              int,                   -- Count of barcode printers
  weighing_machine             int,                   -- Count of weighing machines
  selling_type                 varchar(50),           -- e.g., Retail, Wholesale
  status                       varchar(20),           -- Active/Inactive
  created_by                   int,
  created_at                   datetime,
  updated_by                   int,
  updated_at                   datetime,
  deleted_by                   int,
  deleted_at                   datetime
);
```

#### currencies
Currency management table.
```sql
CREATE TABLE currencies (
  id int PRIMARY KEY AUTO_INCREMENT,
  currency_name varchar(100) NOT NULL,
  currency_code varchar(10) NOT NULL,
  symbol varchar(10) NOT NULL,
  image_path varchar(500),
  exchange_rate decimal(18,4) DEFAULT 1.0000,
  is_default boolean DEFAULT false,
  created_at datetime DEFAULT CURRENT_TIMESTAMP NOT NULL,
  updated_at datetime DEFAULT CURRENT_TIMESTAMP NOT NULL
);
```

---

### 🌍 Geography & Locations

#### countries
Stores country information for addresses and localization.
```sql
CREATE TABLE countries (
  country_id         bigint PRIMARY KEY AUTO_INCREMENT,                
  country_name       varchar(100) NOT NULL,               
  isd                varchar(10),                           
  currency_code      varchar(3),                            
  currency_symbol    varchar(10),                           
  flag_icon          varchar(255),                          
  country_code4      varchar(4),                            
  status             boolean DEFAULT true,               
  created_by         bigint,              
  created_at         datetime DEFAULT CURRENT_TIMESTAMP,
  updated_by         bigint,              
  updated_at         datetime,
  deleted_by         bigint,              
  deleted_at         datetime
);
```

#### states
Stores state/province information for addresses.
```sql
CREATE TABLE states (
  state_id           bigint PRIMARY KEY AUTO_INCREMENT,                
  country_id         bigint NOT NULL, 
  state_name         varchar(100) NOT NULL,               
  state_code         varchar(10),                           
  status             boolean DEFAULT true,               
  created_by         bigint,              
  created_at         datetime DEFAULT CURRENT_TIMESTAMP,
  updated_by         bigint,              
  updated_at         datetime,
  deleted_by         bigint,              
  deleted_at         datetime
);
```

#### cities
Stores city information for addresses.
```sql
CREATE TABLE cities (
  city_id            bigint PRIMARY KEY AUTO_INCREMENT,                
  state_id           bigint NOT NULL, 
  city_name          varchar(100) NOT NULL,               
  postal_code        varchar(20),                           
  status             boolean DEFAULT true,               
  created_by         bigint,              
  created_at         datetime DEFAULT CURRENT_TIMESTAMP,
  updated_by         bigint,              
  updated_at         datetime,
  deleted_by         bigint,              
  deleted_at         datetime
);
```

---

### 👥 User Management & Security

#### users
Stores user accounts for the POS system.
```sql
CREATE TABLE users (
  id               bigint PRIMARY KEY AUTO_INCREMENT,
  deleted          boolean DEFAULT false,
  owner_id         bigint,
  full_name        varchar(100),
  email            varchar(100) UNIQUE,
  password         varchar(255),
  role             varchar(50),
  phone_no         varchar(20),
  salary           decimal(10,2),
  dob              date,
  nationality_status enum('inactive','active'),
  role_permission_id int NOT NULL,
  shopid           int NOT NULL,
  change_access    boolean DEFAULT false,
  shift_type_id    bigint,                               -- NEW FK
  address          text,
  additional_details text,
  uae_id           varchar(50) UNIQUE,
  created_at timestamp DEFAULT NOW()
);
```

#### roles
Defines user roles in the system.
```sql
CREATE TABLE roles (
  role_id int PRIMARY KEY AUTO_INCREMENT,
  role_name varchar(100) NOT NULL,
  description text,
  status varchar(20) DEFAULT 'Active',
  created_by int,
  created_at timestamp DEFAULT NOW(),
  updated_by int,
  updated_at timestamp DEFAULT NOW(),
  deleted_at timestamp,
  deleted_by int
);
```

#### permissions
Stores individual system permissions.
```sql
CREATE TABLE permissions (
  permission_id int PRIMARY KEY AUTO_INCREMENT,
  name varchar(100) NOT NULL,
  code varchar(50) UNIQUE NOT NULL,
  screen_name varchar(100),                             -- UI screen this applies to
  type_matrix varchar(20),                              -- CRUD operations: Create,Read,Update,Delete
  is_parent boolean DEFAULT false,
  parent_permission_id int,                             -- For hierarchical permissions
  status varchar(20) DEFAULT 'Active',
  created_by int,
  created_at timestamp DEFAULT NOW(),
  updated_by int,
  updated_at timestamp DEFAULT NOW(),
  deleted_at timestamp,
  deleted_by int
);
```

#### roles_permission
Links roles to their permissions.
```sql
CREATE TABLE roles_permission (
  role_permission_id int PRIMARY KEY AUTO_INCREMENT,
  role_id int NOT NULL,
  permission_id int NOT NULL,
  status varchar(20) DEFAULT 'Active',
  created_by int,
  created_at timestamp DEFAULT NOW(),
  updated_by int,
  updated_at timestamp DEFAULT NOW(),
  deleted_at timestamp,
  deleted_by int,
  UNIQUE KEY unique_role_permission (role_id, permission_id)
);
```

#### user_permission_overrides
Stores temporary permission exceptions for users.
```sql
CREATE TABLE user_permission_overrides (
  id int PRIMARY KEY AUTO_INCREMENT,
  user_id int NOT NULL,
  permission_id int NOT NULL,
  is_allowed boolean DEFAULT true,
  reason text,
  valid_from timestamp,
  valid_to timestamp,
  created_by int,
  created_at timestamp DEFAULT NOW(),
  UNIQUE KEY unique_user_permission (user_id, permission_id)
);
```

---

### 📦 Product Management

#### category
Organizes products into hierarchical categories.
```sql
CREATE TABLE category (
  id int PRIMARY KEY AUTO_INCREMENT,
  name varchar(100) NOT NULL,
  name_ar varchar(100),
  parent_id int,
  description text,
  image_url text,
  display_order int DEFAULT 0,
  status varchar(20) DEFAULT 'Active',
  created_by int,
  created_at timestamp DEFAULT NOW(),
  updated_at timestamp DEFAULT NOW(),
  INDEX idx_parent_id (parent_id),
  INDEX idx_display_order (display_order)
);
```

#### category_translation
Provides multilingual support for category names.
```sql
CREATE TABLE category_translation (
  id int PRIMARY KEY AUTO_INCREMENT,
  category_id int NOT NULL,
  language_code varchar(10) NOT NULL,
  name varchar(100) NOT NULL,
  description text,
  created_at timestamp DEFAULT NOW(),
  UNIQUE KEY unique_category_language (category_id, language_code)
);
```

#### product
Core product table with minimal fields.
```sql
CREATE TABLE product (
  id int PRIMARY KEY AUTO_INCREMENT,
  deleted int NOT NULL DEFAULT 0,
  created_at timestamp NOT NULL DEFAULT NOW(),
  updated_at timestamp NOT NULL DEFAULT NOW(),
  status varchar(20) DEFAULT 'Active',
  type varchar(20)                                      -- 'Physical', 'Digital', 'Service'
);
```

#### product_info
Extended product information.
```sql
CREATE TABLE product_info (
  id int PRIMARY KEY AUTO_INCREMENT,
  product_id int NOT NULL UNIQUE,
  
  product_name varchar(100) NOT NULL,
  product_name_ar varchar(100),
  alternate_name varchar(100),
  alternate_name_ar varchar(100),
  full_description text,
  full_description_ar text,
  short_description text,
  short_description_ar text,

  sku varchar(100) UNIQUE NOT NULL,
  model_number varchar(100),
  created_barcode boolean NOT NULL DEFAULT false,
  has_standard_barcode boolean NOT NULL DEFAULT true,

  category_id int NOT NULL,
  sub_category_lvl1_id int,
  sub_category_lvl2_id int,
  brand_id int,

  product_unit varchar(50) NOT NULL,
  weight decimal(10,2),
  dimensions varchar(100),
  specs_flag boolean NOT NULL DEFAULT true,
  specs text,
  color varchar(50),

  reorder_level int NOT NULL DEFAULT 0,
  store_location varchar(200),
  can_return boolean NOT NULL DEFAULT false,
  country_of_origin varchar(100),
  
  -- New fields added
  supplier_id int,
  shop_location_id int,
  stock_unit_id int,
  purchase_unit_id int,
  selling_unit_id int,
  with_expiry_date boolean DEFAULT false,
  expiry_days int,                                      -- For products with expiry
  has_warranty boolean DEFAULT false,
  warranty_period int,                                  -- In months
  warranty_type_id int,
  price_type varchar(20),                               -- 'Fixed', 'Variable', 'Tiered'

  created_at timestamp NOT NULL DEFAULT NOW(),
  updated_at timestamp NOT NULL DEFAULT NOW()
);
```

#### product_quantity_history
Tracks product stock quantity history.
```sql
CREATE TABLE product_quantity_history (
  id int PRIMARY KEY AUTO_INCREMENT,
  product_id int,
  quantity decimal(10,2),                               -- Stock Quantity
  reorder_level int,                                    -- Reorder Level
  created_at timestamp DEFAULT NOW()
);
```

#### brand
Stores product brand information.
```sql
CREATE TABLE brand (
  id int PRIMARY KEY AUTO_INCREMENT,
  deleted int,
  name varchar(100) NOT NULL UNIQUE,                    -- Brand Name
  name_arabic varchar(100),                             -- Optional translated name
  description text,                                     -- Optional brand details
  logo_url text,                                        -- Optional brand logo
  created_at timestamp DEFAULT NOW(),
  updated_at timestamp DEFAULT NOW()
);
```

#### product_attributes
Defines product attributes like size, color etc.
```sql
CREATE TABLE product_attributes (
  id int PRIMARY KEY AUTO_INCREMENT,
  name varchar(100) NOT NULL,
  name_ar varchar(100),
  is_required boolean DEFAULT false,
  type varchar(20),                                     -- 'Color', 'Size', 'Material', 'Custom'
  status varchar(20) DEFAULT 'Active',
  created_by int NOT NULL,
  created_at timestamp DEFAULT NOW(),
  updated_at timestamp DEFAULT NOW()
);
```

#### product_attribute_values
Stores possible values for each attribute.
```sql
CREATE TABLE product_attribute_values (
  id int PRIMARY KEY AUTO_INCREMENT,
  attribute_id int NOT NULL,
  value varchar(100) NOT NULL,
  value_ar varchar(100),
  status varchar(20) DEFAULT 'Active',
  created_at timestamp DEFAULT NOW()
);
```

#### product_attribute_options
Links products to their attribute options.
```sql
CREATE TABLE product_attribute_options (
  id int PRIMARY KEY AUTO_INCREMENT,
  product_id int NOT NULL,
  attribute_id int NOT NULL,
  value_id int NOT NULL,
  price_adjustment decimal(10,2) DEFAULT 0,
  status varchar(20) DEFAULT 'Active',
  created_at timestamp DEFAULT NOW(),
  UNIQUE KEY unique_product_attribute_value (product_id, attribute_id, value_id)
);
```

#### product_combinations
Stores predefined combinations of attributes.
```sql
CREATE TABLE product_combinations (
  id int PRIMARY KEY AUTO_INCREMENT,
  product_id int NOT NULL,
  combination_string varchar(255) NOT NULL,
  sku varchar(100),
  barcode varchar(50),
  price decimal(12,2),
  cost_price decimal(12,2),
  quantity decimal(12,4) DEFAULT 0,
  image_url text,
  is_active boolean DEFAULT true,
  created_at timestamp DEFAULT NOW(),
  INDEX idx_product_id (product_id),
  INDEX idx_sku (sku),
  INDEX idx_barcode (barcode)
);
```

#### product_combination_items
Stores components of each product combination.
```sql
CREATE TABLE product_combination_items (
  id int PRIMARY KEY AUTO_INCREMENT,
  combination_id int NOT NULL,
  attribute_id int NOT NULL,
  value_id int NOT NULL,
  created_at timestamp DEFAULT NOW(),
  INDEX idx_combination_id (combination_id),
  UNIQUE KEY unique_combination_attribute (combination_id, attribute_id)
);
```

#### product_barcodes
Stores multiple barcode support per product.
```sql
CREATE TABLE product_barcodes (
  id int PRIMARY KEY AUTO_INCREMENT,
  product_id int NOT NULL,
  barcode varchar(100) UNIQUE NOT NULL,
  barcode_type varchar(20) NOT NULL DEFAULT 'ean',
  created_at timestamp NOT NULL DEFAULT NOW()
);
```

#### product_images
Stores product images with ordering support.
```sql
CREATE TABLE product_images (
  id int PRIMARY KEY AUTO_INCREMENT,
  product_id int NOT NULL,
  image_url text NOT NULL,
  alt_text varchar(255),
  sort_order int NOT NULL DEFAULT 0,
  is_primary boolean DFEFAULT false,
  created_at timestamp NOT NULL DEFAULT NOW()
);
```

#### product_modifiers
Stores individual product modifiers/add-ons.
```sql
CREATE TABLE product_modifiers (
  id int PRIMARY KEY AUTO_INCREMENT,
  name varchar(100) NOT NULL,
  description text,
  price decimal(10,2) DEFAULT 0,
  cost decimal(10,2) DEFAULT 0,
  sku varchar(50),
  barcode varchar(50),
  tax_type_id int,
  status varchar(20) DEFAULT 'Active',
  created_by int NOT NULL,
  created_at timestamp DEFAULT NOW(),
  updated_at timestamp DEFAULT NOW()
);
```

#### product_modifier_groups
Stores groups of modifiers (e.g., Pizza Toppings).
```sql
CREATE TABLE product_modifier_groups (
  id int PRIMARY KEY AUTO_INCREMENT,
  name varchar(100) NOT NULL,
  description text,
  selection_type varchar(20),                           -- 'Single', 'Multiple'
  required boolean DEFAULT false,
  min_selections int DEFAULT 0,
  max_selections int,
  status varchar(20) DEFAULT 'Active',
  created_by int NOT NULL,
  created_at timestamp DEFAULT NOW(),
  updated_at timestamp DEFAULT NOW()
);
```

#### product_modifier_group_items
Links modifiers to modifier groups.
```sql
CREATE TABLE product_modifier_group_items (
  id int PRIMARY KEY AUTO_INCREMENT,
  group_id int NOT NULL,
  modifier_id int NOT NULL,
  price_adjustment decimal(10,2) DEFAULT 0,
  sort_order int DEFAULT 0,
  default_selection boolean DEFAULT false,
  status varchar(20) DEFAULT 'Active',
  created_at timestamp DEFAULT NOW()
);
```

#### product_modifier_links
Links products to modifier groups.
```sql
CREATE TABLE product_modifier_links (
  id int PRIMARY KEY AUTO_INCREMENT,
  product_id int NOT NULL,
  modifier_group_id int NOT NULL,
  created_at timestamp DEFAULT NOW(),
  UNIQUE KEY unique_product_modifier_group (product_id, modifier_group_id)
);
```

#### modifiers
Master table for product modifiers.
```sql
CREATE TABLE modifiers (
  id            int PRIMARY KEY AUTO_INCREMENT,
  name          varchar(100) NOT NULL,
  description   text,
  extra_price   decimal(10,2) NOT NULL DEFAULT 0.00,
  is_active     boolean NOT NULL DEFAULT true,
  deleted       int NOT NULL DEFAULT 0,
  created_by    int NOT NULL,
  created_at    timestamp NOT NULL DEFAULT NOW(),
  updated_at    timestamp NOT NULL DEFAULT NOW()
);
```

---

### 📊 Inventory Management

#### product_batches
Tracks batch information for inventory.
```sql
CREATE TABLE product_batches (
  id int PRIMARY KEY AUTO_INCREMENT,
  product_id int NOT NULL,
  batch_no varchar(50) NOT NULL,
  manufacture_date date,
  expiry_date date,
  quantity decimal(12,4) NOT NULL,
  uom_id int NOT NULL,
  cost_price decimal(12,2),
  landed_cost decimal(12,2),
  status varchar(20) DEFAULT 'Active',
  created_at timestamp DEFAULT NOW(),
  UNIQUE KEY unique_product_batch (product_id, batch_no),
  INDEX idx_expiry_date (expiry_date)
);
```

#### stock_movement
Tracks detailed stock movements.
```sql
CREATE TABLE stock_movement (
  id int PRIMARY KEY AUTO_INCREMENT,
  product_id int NOT NULL,
  batch_id int,
  uom_id int NOT NULL,
  movement_type varchar(20),                            -- 'Purchase', 'Sale', 'Transfer', 'Adjustment', 'Waste'
  quantity decimal(12,4) NOT NULL,
  reference_type varchar(50),                           -- 'PurchaseOrder', 'Sale', 'Transfer', 'Adjustment'
  reference_id int NOT NULL,
  location_id int,
  notes text,
  created_by int NOT NULL,
  created_at timestamp DEFAULT NOW(),
  INDEX idx_product_id (product_id),
  INDEX idx_reference_type (reference_type),
  INDEX idx_reference_id (reference_id)
);
```

#### goods_received
Goods receipt management.
```sql
CREATE TABLE goods_received (
  id int PRIMARY KEY AUTO_INCREMENT,
  grn_no varchar(50) NOT NULL,
  supplier_id bigint NOT NULL,
  store_id int NOT NULL,
  invoice_no varchar(50),
  invoice_date datetime,
  received_date datetime NOT NULL DEFAULT CURRENT_DATE,
  total_amount decimal(12,2) DEFAULT 0,
  remarks varchar(255),
  status varchar(20) DEFAULT 'Pending',
  created_at datetime DEFAULT CURRENT_TIMESTAMP NOT NULL
);
```

#### goods_received_items
GRN line items.
```sql
CREATE TABLE goods_received_items (
  id int PRIMARY KEY AUTO_INCREMENT,
  grn_id int NOT NULL,
  product_id int NOT NULL,
  batch_id int,
  batch_no varchar(50),
  manufacture_date datetime,
  expiry_date datetime,
  quantity decimal(12,4) NOT NULL,
  uom_id bigint NOT NULL,
  cost_price decimal(12,2) NOT NULL,
  landed_cost decimal(12,2),
  line_total decimal(12,2),
  created_at datetime DEFAULT CURRENT_TIMESTAMP NOT NULL
);
```

#### goods_returns
Return to supplier tracking.
```sql
CREATE TABLE goods_returns (
  id int PRIMARY KEY AUTO_INCREMENT,
  return_no varchar(50) NOT NULL,
  supplier_id bigint NOT NULL,
  store_id int NOT NULL,
  reference_grn_id int,
  return_date datetime NOT NULL,
  total_amount decimal(12,2) DEFAULT 0,
  status varchar(20) DEFAULT 'Pending',
  remarks varchar(255),
  is_totally_replaced boolean DEFAULT false,
  created_by int NOT NULL,
  created_at datetime DEFAULT CURRENT_TIMESTAMP NOT NULL,
  updated_at datetime DEFAULT CURRENT_TIMESTAMP NOT NULL
);
```

#### goods_return_items
Return line items.
```sql
CREATE TABLE goods_return_items (
  id int PRIMARY KEY AUTO_INCREMENT,
  return_id int NOT NULL,
  product_id int NOT NULL,
  batch_id int,
  batch_no varchar(50),
  expiry_date datetime,
  quantity decimal(12,4) NOT NULL,
  uom_id bigint NOT NULL,
  cost_price decimal(12,2) NOT NULL,
  line_total decimal(12,2),
  reason varchar(255),
  already_replaced_quantity decimal(12,4) DEFAULT 0,
  is_totally_replaced boolean DEFAULT false,
  created_at datetime DEFAULT CURRENT_TIMESTAMP NOT NULL
);
```

#### goods_replaces
Replacement goods tracking.
```sql
CREATE TABLE goods_replaces (
  id int PRIMARY KEY AUTO_INCREMENT,
  replace_no varchar(30) NOT NULL,
  supplier_id int NOT NULL,
  store_id int NOT NULL,
  reference_return_id int,
  replace_date datetime NOT NULL,
  total_amount decimal(12,2) DEFAULT 0,
  status varchar(20) DEFAULT 'Pending',
  remarks varchar(255),
  created_by int NOT NULL,
  created_at datetime DEFAULT CURRENT_TIMESTAMP NOT NULL,
  updated_at datetime DEFAULT CURRENT_TIMESTAMP NOT NULL
);
```

#### goods_replace_items
Replacement line items.
```sql
CREATE TABLE goods_replace_items (
  id int PRIMARY KEY AUTO_INCREMENT,
  replace_id int NOT NULL,
  product_id int NOT NULL,
  uom_id bigint NOT NULL,
  batch_no varchar(50),
  expiry_date datetime,
  quantity decimal(12,4) NOT NULL,
  rate decimal(12,2) NOT NULL,
  amount decimal(12,2),
  reference_return_item_id int,
  remarks_line varchar(255),
  created_at datetime DEFAULT CURRENT_TIMESTAMP NOT NULL
);
```

#### stock_transfers
Inter-location transfers.
```sql
CREATE TABLE stock_transfers (
  transfer_id int PRIMARY KEY AUTO_INCREMENT,
  transfer_no varchar(30) NOT NULL,
  transfer_date datetime NOT NULL,
  from_store_id int NOT NULL,
  to_store_id int NOT NULL,
  status varchar(20) DEFAULT 'Pending',
  remarks text,
  created_by int NOT NULL,
  created_at datetime DEFAULT CURRENT_TIMESTAMP NOT NULL,
  updated_by int,
  updated_at datetime
);
```

#### stock_transfer_items
Stock transfer line items.
```sql
CREATE TABLE stock_transfer_items (
  id int PRIMARY KEY AUTO_INCREMENT,
  transfer_id int NOT NULL,
  product_id int NOT NULL,
  uom_id bigint NOT NULL,
  batch_no varchar(50),
  expiry_date datetime,
  quantity_sent decimal(12,4) NOT NULL,
  quantity_received decimal(12,4) DEFAULT 0,
  damaged_qty decimal(12,4) DEFAULT 0,
  status varchar(20) DEFAULT 'Pending',
  remarks_line text
);
```

#### stock_adjustments
Stock adjustment records.
```sql
CREATE TABLE stock_adjustments (
  adjustment_id int PRIMARY KEY AUTO_INCREMENT,
  adjustment_no varchar(30) NOT NULL,
  adjustment_date datetime NOT NULL,
  store_location_id int NOT NULL,
  reason_id int NOT NULL,
  status varchar(20) DEFAULT 'Pending',
  remarks text,
  created_by int NOT NULL,
  created_at datetime DEFAULT CURRENT_TIMESTAMP NOT NULL,
  updated_by int,
  updated_at datetime
);
```

#### stock_adjustment_reasons
Adjustment reason codes.
```sql
CREATE TABLE stock_adjustment_reasons (
  stock_adjustment_reasons_id int PRIMARY KEY AUTO_INCREMENT,
  name varchar(255) NOT NULL,
  description varchar(255),
  status varchar(255) DEFAULT 'Active',
  created_by int,
  created_at datetime DEFAULT CURRENT_TIMESTAMP,
  updated_by int,
  updated_at datetime,
  deleted_at datetime
);
```

#### stock_adjustment_items
Stock adjustment line items.
```sql
CREATE TABLE stock_adjustment_items (
  id int PRIMARY KEY AUTO_INCREMENT,
  adjustment_id int NOT NULL,
  product_id int NOT NULL,
  uom_id bigint NOT NULL,
  batch_no varchar(50),
  expiry_date datetime,
  quantity_before decimal(12,4) NOT NULL,
  quantity_after decimal(12,4) NOT NULL,
  difference_qty decimal(12,4) NOT NULL,
  conversion_factor decimal(12,4) DEFAULT 1,
  reason_line varchar(100),
  remarks_line text
);
```

#### stock_ledger
Stock movement history ledger.
```sql
CREATE TABLE stock_ledger (
  id int PRIMARY KEY AUTO_INCREMENT,
  product_id int NOT NULL,
  unit_id int NOT NULL,
  movement_type varchar(20) NOT NULL,
  qty decimal(10,2) NOT NULL,
  balance decimal(10,2) NOT NULL,
  location varchar(200),
  reference_type varchar(50),
  reference_id int,
  created_at timestamp NOT NULL DEFAULT NOW()
);
```

---

### 🏢 Supplier Management

#### supplier
Master table for supplier information.
```sql
CREATE TABLE supplier (
  supplier_id bigint PRIMARY KEY AUTO_INCREMENT,
  shop_id bigint,
  company_name varchar(100) NOT NULL,
  logo_picture varchar(255),
  license_number varchar(50),
  owner_name varchar(100),
  owner_mobile varchar(20),
  vat_trn_number varchar(50),
  email varchar(100) UNIQUE,
  address_line1 varchar(255) NOT NULL,
  address_line2 varchar(255),
  building varchar(100),
  area varchar(100),
  po_box varchar(20),
  city varchar(100),
  state varchar(100),
  country varchar(100),
  website varchar(100),
  key_contact_name varchar(100),
  key_contact_mobile varchar(20),
  key_contact_email varchar(100),
  mobile varchar(20),
  location_latitude decimal(10,8),
  location_longitude decimal(11,8),
  company_phone_number varchar(20),
  gstin varchar(20),
  pan varchar(20),
  payment_terms varchar(50),
  opening_balance decimal(12,2) DEFAULT 0.00,
  balance_type enum('credit', 'debit') DEFAULT 'credit',
  status varchar(20) DEFAULT 'Active',
  created_by bigint,
  created_at timestamp DEFAULT NOW(),
  updated_by bigint,
  updated_at timestamp DEFAULT NOW(),
  deleted_at timestamp,
  deleted_by bigint,
  INDEX idx_company_name (company_name),
  INDEX idx_vat_trn_number (vat_trn_number),
  INDEX idx_email (email)
);
```

#### supplier_translation
Multilingual supplier information.
```sql
CREATE TABLE supplier_translation (
  id bigint PRIMARY KEY AUTO_INCREMENT,
  supplier_id bigint NOT NULL,
  language_id int NOT NULL,
  supplier_name_ar varchar(100),
  address_line1_ar varchar(255),
  address_line2_ar varchar(255),
  building_ar varchar(100),
  area_ar varchar(100),
  status varchar(20) DEFAULT 'Active',
  created_by bigint,
  created_at timestamp DEFAULT NOW(),
  updated_by bigint,
  updated_at timestamp DEFAULT NOW(),
  deleted_at timestamp,
  deleted_by bigint,
  UNIQUE KEY unique_supplier_language (supplier_id, language_id)
);
```

#### supplier_bank_accounts
Supplier banking information with multiple account support.
```sql
CREATE TABLE supplier_bank_accounts (
  id bigint PRIMARY KEY AUTO_INCREMENT,
  supplier_id bigint NOT NULL,
  bank_name varchar(100) NOT NULL,
  account_number varchar(50) NOT NULL,
  iban_ifsc varchar(50),
  swift_code varchar(20),
  branch_name varchar(100),
  account_type varchar(50),                             -- Current/Savings/etc.
  branch_address text,
  bank_key_person_name varchar(100),
  bank_key_contacts varchar(100),
  notes text,
  is_primary boolean DEFAULT false,
  status varchar(20) DEFAULT 'Active',
  created_by bigint,
  created_at timestamp DEFAULT NOW(),
  updated_by bigint,
  updated_at timestamp DEFAULT NOW(),
  deleted_at timestamp,
  deleted_by bigint,
  INDEX idx_supplier_account (supplier_id, account_number)
);
```

#### supplier_purchase
Supplier purchase orders/invoices.
```sql
CREATE TABLE supplier_purchase (
  purchase_id bigint PRIMARY KEY AUTO_INCREMENT,
  supplier_id bigint NOT NULL,
  invoice_no varchar(50) NOT NULL,
  invoice_date date NOT NULL,
  due_date date,
  total_amount decimal(12,2) NOT NULL,
  tax_amount decimal(12,2),
  discount_amount decimal(12,2),
  paid_amount decimal(12,2) DEFAULT 0.00,
  due_amount decimal(12,2),
  status enum('paid', 'partial', 'unpaid', 'cancelled') DEFAULT 'unpaid',
  payment_terms varchar(100),
  delivery_terms varchar(100),
  remarks text,
  created_by bigint,
  created_at timestamp DEFAULT NOW(),
  updated_by bigint,
  updated_at timestamp DEFAULT NOW(),
  UNIQUE KEY unique_invoice_no (invoice_no),
  INDEX idx_supplier_id (supplier_id),
  INDEX idx_invoice_date (invoice_date)
);
```

#### supplier_purchase_item
Items in supplier purchases with receipt tracking.
```sql
CREATE TABLE supplier_purchase_item (
  item_id bigint PRIMARY KEY AUTO_INCREMENT,
  purchase_id bigint NOT NULL,
  product_id int NOT NULL,
  product_variant_id int,
  quantity decimal(10,3) NOT NULL,
  unit_price decimal(10,2) NOT NULL,
  tax_rate decimal(5,2),
  discount_rate decimal(5,2),
  total_price decimal(12,2) NOT NULL,
  received_quantity decimal(10,3),
  status varchar(20) DEFAULT 'Active'
);
```

---

### 🛡️ Warranty Management

#### warranty_type
Master table for all warranty types.
```sql
CREATE TABLE warranty_type (
  id int PRIMARY KEY AUTO_INCREMENT,
  name varchar(100) NOT NULL,
  name_ar varchar(100),
  is_additional boolean DEFAULT false,
  cost decimal(10,2),                                   -- Only when is_additional=true
  description text,
  status varchar(20) DEFAULT 'Active',
  created_by int NOT NULL,
  created_at timestamp DEFAULT NOW(),
  updated_by int,
  updated_at timestamp NOT NULL DEFAULT NOW(),
  deleted_at timestamp,
  deleted_by int
);
```

#### product_warranty
Links products to their available warranties.
```sql
CREATE TABLE product_warranty (
  id int PRIMARY KEY AUTO_INCREMENT,
  product_id int NOT NULL,
  warranty_type_id int NOT NULL,
  duration_months int NOT NULL,
  terms_conditions text,
  is_included boolean DEFAULT true,
  created_by int NOT NULL,
  created_at timestamp DEFAULT NOW(),
  updated_by int,
  updated_at timestamp NOT NULL DEFAULT NOW(),
  UNIQUE KEY unique_product_warranty (product_id, warranty_type_id)
);
```

#### sales_warranty
Tracks warranties actually sold to customers.
```sql
CREATE TABLE sales_warranty (
  id int PRIMARY KEY AUTO_INCREMENT,
  sale_id int NOT NULL,
  product_id int NOT NULL,
  warranty_type_id int NOT NULL,
  duration_months int NOT NULL,
  start_date date NOT NULL,
  end_date date NOT NULL,
  cost decimal(10,2) DEFAULT 0,
  status varchar(20) DEFAULT 'Active',
  created_at timestamp DEFAULT NOW(),
  INDEX idx_sale_product (sale_id, product_id),
  INDEX idx_end_date (end_date)
);
```

#### warranty_claims
Tracks customer warranty claims.
```sql
CREATE TABLE warranty_claims (
  id int PRIMARY KEY AUTO_INCREMENT,
  sales_warranty_id int NOT NULL,
  claim_date date NOT NULL,
  description text NOT NULL,
  resolution text,
  status varchar(20) DEFAULT 'Pending',
  resolved_by int,
  resolved_at timestamp,
  created_at timestamp DEFAULT NOW()
);
```

#### product_variants
Different packaging/sizing variants of products.
```sql
CREATE TABLE product_variants (
  variant_id int PRIMARY KEY AUTO_INCREMENT,
  product_id int NOT NULL,
  unit_option_id int NOT NULL,
  value decimal(12,4) NOT NULL,
  label varchar(100) NOT NULL,
  barcode varchar(50) UNIQUE,
  price decimal(12,2) NOT NULL,
  cost_price decimal(12,2) NOT NULL,
  stock_quantity decimal(12,4) DEFAULT 0,
  status varchar(20) DEFAULT 'Active',
  created_by int NOT NULL,
  created_at timestamp DEFAULT NOW(),
  updated_by int,
  updated_at timestamp DEFAULT NOW(),
  deleted_at timestamp,
  deleted_by int,
  INDEX idx_product_id (product_id),
  INDEX idx_unit_option_id (unit_option_id),
  INDEX idx_barcode (barcode)
);
```

---

### 🏪 Company Management

#### companies
Main company information table.
```sql
CREATE TABLE companies (
  company_id int PRIMARY KEY AUTO_INCREMENT,
  company_name varchar(100) NOT NULL,
  logo_picture varchar(255),
  license_number varchar(50) UNIQUE,
  vat_trn_number varchar(50),
  phone_number varchar(20),
  email varchar(100) UNIQUE,
  website varchar(100),
  key_contact_name varchar(100),
  key_contact_mobile varchar(20),
  key_contact_email varchar(100),
  location_latitude decimal(10,8),
  location_longitude decimal(11,8),
  remarks text,
  status varchar(20) DEFAULT 'active',
  created_by int,
  created_at timestamp DEFAULT NOW(),
  updated_by int,
  updated_at timestamp DEFAULT NOW(),
  deleted_at timestamp,
  deleted_by int
);
```

#### company_owners
Company owners/partners information.
```sql
CREATE TABLE company_owners (
  owner_id int PRIMARY KEY AUTO_INCREMENT,
  company_id int,
  owner_first_name varchar(50) NOT NULL,
  owner_last_name varchar(50) NOT NULL,
  auth_number varchar(50),
  owner_mobile varchar(20),
  owner_uae_id varchar(50),
  owner_email varchar(100),
  status varchar(20) DEFAULT 'active',
  created_by int,
  created_at timestamp DEFAULT NOW(),
  updated_by int,
  updated_at timestamp DEFAULT NOW(),
  deleted_at timestamp,
  deleted_by int
);
```

#### company_addresses
Physical addresses for companies.
```sql
CREATE TABLE company_addresses (
  company_address_id int PRIMARY KEY AUTO_INCREMENT,
  company_id int,
  address_line1 varchar(255) NOT NULL,
  address_line2 varchar(255),
  area varchar(100),
  po_box varchar(20),
  city varchar(100),
  state_id int,
  country_id int,
  is_billing_address boolean DEFAULT false,
  location_latitude decimal(10,8),
  location_longitude decimal(11,8),
  status varchar(20) DEFAULT 'active',
  created_by int,
  created_at timestamp DEFAULT NOW(),
  updated_by int,
  updated_at timestamp DEFAULT NOW(),
  deleted_at timestamp,
  deleted_by int
);
```

#### company_bank_accounts
Bank account information for companies.
```sql
CREATE TABLE company_bank_accounts (
  id int PRIMARY KEY AUTO_INCREMENT,
  company_id int,
  company_address_id int,
  account_type varchar(50),                             -- Current/Savings/etc.
  account_name varchar(100) NOT NULL,
  account_number varchar(50) NOT NULL,
  iban varchar(50),
  swift_code varchar(20),
  branch_name varchar(100),
  branch_address text,
  status varchar(20) DEFAULT 'active',
  created_by int,
  created_at timestamp DEFAULT NOW(),
  updated_by int,
  updated_at timestamp DEFAULT NOW(),
  deleted_at timestamp,
  deleted_by int
);
```

---

### 📄 License Management

#### license_info
License information for software access.
```sql
CREATE TABLE license_info (
  id varchar(36) PRIMARY KEY,                          -- UUID
  company_id int,
  license_key varchar(100) UNIQUE NOT NULL,
  organization_name varchar(255),
  issued_to varchar(255),
  issued_on timestamp NOT NULL,
  valid_until timestamp NOT NULL,
  device_limit int,
  allowed_macs text,                                   -- Comma-separated MAC addresses
  allowed_ips text,                                    -- Comma-separated IP addresses
  license_type varchar(50),                            -- PER_DEVICE/PER_LOCATION/UNLIMITED
  is_active boolean DEFAULT true,
  plan_id int,
  notes text,
  created_at timestamp DEFAULT NOW(),
  updated_at timestamp DEFAULT NOW()
);
```

#### license_modules
Enabled modules for each license.
```sql
CREATE TABLE license_modules (
  id varchar(36) PRIMARY KEY,                          -- UUID
  license_id varchar(36),
  module_name varchar(100) NOT NULL,
  is_enabled boolean DEFAULT true,
  created_at timestamp DEFAULT NOW()
);
```

#### license_device_usage
Track per-device usage for licensing compliance.
```sql
CREATE TABLE license_device_usage (
  id varchar(36) PRIMARY KEY,                          -- UUID
  license_id varchar(36),
  mac_address varchar(50) NOT NULL,
  ip_address varchar(50),
  device_name varchar(100),
  last_checked_in timestamp,
  created_at timestamp DEFAULT NOW()
);
```

---

### 👤 Customer Management

#### customer
Stores customer information for the POS system.
```sql
CREATE TABLE customer (
  id int PRIMARY KEY AUTO_INCREMENT,
  customer_full_name varchar(150),
  business_full_name varchar(150),
  is_business boolean,
  business_type_id int,
  customer_balance_amount decimal,
  license_no varchar(50),
  trn_no varchar(50),
  mobile_no varchar(20),
  home_phone varchar(20),
  office_phone varchar(20),
  contact_mobile_no varchar(20),
  credit_allowed boolean,
  credit_amount_max decimal,
  credit_days int,
  credit_reference1_name varchar(150),
  credit_reference2_name varchar(150),
  key_contact_name varchar(150),
  key_contact_mobile varchar(20),
  key_contact_email varchar(100),
  finance_person_name varchar(150),
  finance_person_mobile varchar(20),
  finance_person_email varchar(100),
  official_email varchar(100),
  post_dated_cheques_allowed boolean,
  shop_id int,
  status varchar(20),
  created_by int,
  created_at timestamp,
  updated_by int,
  updated_at timestamp,
  deleted_at timestamp,
  deleted_by int
);
```

#### business_type_master
Defines types of businesses for customer categorization.
```sql
CREATE TABLE business_type_master (
  id int PRIMARY KEY AUTO_INCREMENT,
  business_type_name varchar(100),
  business_type_name_ar varchar(100),
  status varchar(20),
  created_by int,
  created_at timestamp,
  updated_by int,
  updated_at timestamp,
  deleted_at timestamp,
  deleted_by int
);
```

#### customer_address
Stores customer address information.
```sql
CREATE TABLE customer_address (
  id int PRIMARY KEY AUTO_INCREMENT,
  customer_id int,
  address_line1 varchar(255),
  address_line2 varchar(255),
  po_box varchar(50),
  area varchar(100),
  city varchar(100),
  state_id int,
  country_id int,
  landmark varchar(255),
  latitude decimal,
  longitude decimal,
  is_billing boolean,
  status varchar(20),
  created_by int,
  created_at timestamp,
  updated_by int,
  updated_at timestamp,
  deleted_at timestamp,
  deleted_by int
);
```

#### customer_address_translation
Provides multilingual support for customer addresses.
```sql
CREATE TABLE customer_address_translation (
  id int PRIMARY KEY AUTO_INCREMENT,
  customer_address_id int,
  language_id int,
  address_line1 varchar(255),
  address_line2 varchar(255),
  area varchar(100),
  status varchar(20),
  created_by int,
  created_at timestamp,
  updated_by int,
  updated_at timestamp,
  deleted_at timestamp,
  deleted_by int
);
```

#### customers_groups
Defines customer groups for pricing and promotions.
```sql
CREATE TABLE customers_groups (
  id int PRIMARY KEY AUTO_INCREMENT,
  name varchar(100),
  name_ar varchar(100),
  selling_price_type_id int,
  discount_id int,
  discount_value decimal,
  discount_max_value decimal,
  is_percentage boolean,
  status varchar(20),
  created_by int,
  created_at timestamp,
  updated_by int,
  updated_at timestamp,
  deleted_at timestamp,
  deleted_by int
);
```

#### customers_group_relation
Links customers to customer groups.
```sql
CREATE TABLE customers_group_relation (
  id int PRIMARY KEY AUTO_INCREMENT,
  customer_id int,
  customer_group_id int,
  status varchar(20),
  created_by int,
  created_at timestamp,
  updated_by int,
  updated_at timestamp,
  deleted_at timestamp,
  deleted_by int
);
```

#### customer_balance
Tracks customer account balances.
```sql
CREATE TABLE customer_balance (
  id int PRIMARY KEY AUTO_INCREMENT,
  customer_id int,
  new_balance decimal,
  last_update timestamp,
  status varchar(20),
  created_by int,
  created_at timestamp,
  updated_by int,
  updated_at timestamp,
  deleted_at timestamp,
  deleted_by int
);
```

#### discounts
Stores discount rules and promotions.
```sql
CREATE TABLE discounts (
  id int PRIMARY KEY AUTO_INCREMENT,
  discount_name varchar(150),                           -- Display name of the discount
  discount_name_ar varchar(150),                        -- Arabic name
  discount_code varchar(50) UNIQUE,                     -- Optional coupon/code for identification
  discount_type varchar(20),                            -- 'Percentage' or 'Fixed'
  discount_value decimal(10,2),                         -- Value in percentage or amount
  max_discount_amount decimal(10,2),                    -- Maximum discount limit (if applicable)
  min_purchase_amount decimal(10,2),                    -- Minimum bill amount to apply discount
  start_date date,                                      -- Validity start date
  end_date date,                                        -- Validity end date
  applicable_on varchar(50),                            -- 'All', 'Category', 'Item', 'CustomerGroup'
  applicable_id int,                                    -- Links to category/item/group table based on applicable_on
  is_active boolean DEFAULT true,
  created_by int,
  created_at timestamp,
  updated_by int,
  updated_at timestamp,
  deleted_at timestamp,
  deleted_by int
);
```

#### product_discounts
Product-specific discounts.
```sql
CREATE TABLE product_discounts (
  id int PRIMARY KEY AUTO_INCREMENT,
  product_id int NOT NULL,
  discounts_id int NOT NULL,
  created_by int,
  created_at datetime DEFAULT CURRENT_TIMESTAMP NOT NULL,
  updated_by int,
  updated_at datetime DEFAULT CURRENT_TIMESTAMP NOT NULL,
  deleted_at datetime,
  deleted_by int
);
```

#### category_discounts
Category-level discounts.
```sql
CREATE TABLE category_discounts (
  id int PRIMARY KEY AUTO_INCREMENT,
  category_id int NOT NULL,
  discounts_id int NOT NULL,
  created_by int,
  created_at datetime DEFAULT CURRENT_TIMESTAMP NOT NULL,
  updated_by int,
  updated_at datetime DEFAULT CURRENT_TIMESTAMP NOT NULL,
  deleted_at datetime,
  deleted_by int
);
```

#### customer_discounts
Customer-specific discounts.
```sql
CREATE TABLE customer_discounts (
  id int PRIMARY KEY AUTO_INCREMENT,
  customer_id int NOT NULL,
  discount_id int NOT NULL,
  created_by int,
  created_at datetime DEFAULT CURRENT_TIMESTAMP NOT NULL,
  updated_by int,
  updated_at datetime DEFAULT CURRENT_TIMESTAMP NOT NULL,
  deleted_at datetime,
  deleted_by int
);
```

#### shop_discounts
Shop-specific discounts.
```sql
CREATE TABLE shop_discounts (
  id int PRIMARY KEY AUTO_INCREMENT,
  store_id int NOT NULL,
  discount_id int NOT NULL,
  created_by int,
  created_at datetime DEFAULT CURRENT_TIMESTAMP NOT NULL,
  updated_by int,
  updated_at datetime DEFAULT CURRENT_TIMESTAMP NOT NULL,
  deleted_at datetime,
  deleted_by int
);
```

---

### 💰 Sales & Transactions

#### transactions
Core sales transaction header table.
```sql
CREATE TABLE transactions (
  id                int PRIMARY KEY AUTO_INCREMENT,
  shift_id          int NOT NULL,
  customer_id       int,
  user_id           int NOT NULL,
  shop_location_id  int,
  
  selling_time      timestamp NOT NULL DEFAULT NOW(),
  total_amount      decimal(12,2) NOT NULL DEFAULT 0,
  total_vat         decimal(12,2) NOT NULL DEFAULT 0,
  total_discount    decimal(12,2) NOT NULL DEFAULT 0,
  total_applied_vat decimal(12,2) DEFAULT 0,
  total_applied_discount_value decimal(12,2) DEFAULT 0,
  
  amount_paid_cash  decimal(12,2) DEFAULT 0,
  amount_credit_remaining decimal(12,2) DEFAULT 0,
  credit_days       int DEFAULT 0,
  
  is_percentage_discount boolean DEFAULT false,
  discount_value    decimal(12,2) DEFAULT 0,
  discount_max_value decimal(12,2) DEFAULT 0,
  vat               decimal(12,2) DEFAULT 0,
  discount_note     text,
  invoice_number    varchar(50) UNIQUE,                 -- Format: DDMMYYtransactionID
  
  status            enum('pending','paid','cancelled') DEFAULT 'pending',
  
  created_by        int NOT NULL,
  created_at        timestamp NOT NULL DEFAULT NOW(),
  updated_by        int,
  updated_at        timestamp,
  deleted_at        timestamp,
  deleted_by        int
);
```

#### transaction_products
Line items for sales transactions.
```sql
CREATE TABLE transaction_products (
  id                  int PRIMARY KEY AUTO_INCREMENT,
  transaction_id      int NOT NULL,
  product_id          int NOT NULL,
  shop_location_id    int,
  
  buyer_cost          decimal(12,2) NOT NULL DEFAULT 0,
  selling_price       decimal(12,2) NOT NULL,
  product_unit_id     int,
  
  is_percentage_discount boolean DEFAULT false,
  discount_value      decimal(12,2) DEFAULT 0,
  discount_max_value  decimal(12,2) DEFAULT 0,
  vat                 decimal(12,2) DEFAULT 0,
  quantity            decimal(10,3) NOT NULL,
  
  status              enum('active','returned','exchanged') DEFAULT 'active',
  
  created_by          int,
  created_at          timestamp DEFAULT NOW(),
  updated_by          int,
  updated_at          timestamp,
  deleted_at          timestamp,
  deleted_by          int
);
```

#### transaction_modifiers
Product modifiers applied to transaction items.
```sql
CREATE TABLE transaction_modifiers (
  id                      int PRIMARY KEY AUTO_INCREMENT,
  transaction_product_id  int NOT NULL,
  product_modifier_id     int NOT NULL,
  extra_price             decimal(10,2) NOT NULL DEFAULT 0.00,
  created_by              int,
  created_at              timestamp DEFAULT NOW()
);
```

#### transaction_service_charges
Service charges applied to transactions.
```sql
CREATE TABLE transaction_service_charges (
  id                  int PRIMARY KEY AUTO_INCREMENT,
  transaction_id      int NOT NULL,
  service_charge_id   int NOT NULL,
  total_amount        decimal(12,2) DEFAULT 0,
  total_vat           decimal(12,2) DEFAULT 0,
  status              varchar(20) DEFAULT 'Active',
  created_by          int,
  created_at          timestamp DEFAULT NOW()
);
```

#### refund_transactions
Records for refunded transactions.
```sql
CREATE TABLE refund_transactions (
  id                     int PRIMARY KEY AUTO_INCREMENT,
  customer_id            int,
  selling_transaction_id int NOT NULL,
  shift_id               int,
  user_id                int,
  total_amount           decimal(12,2) DEFAULT 0,
  total_vat              decimal(12,2) DEFAULT 0,
  is_cash                boolean DEFAULT true,
  refund_time            timestamp DEFAULT NOW(),
  status                 varchar(20) DEFAULT 'Active',
  created_by             int,
  created_at             timestamp DEFAULT NOW()
);
```

#### refund_transaction_products
Line items for refunded products.
```sql
CREATE TABLE refund_transaction_products (
  id                      int PRIMARY KEY AUTO_INCREMENT,
  refund_transaction_id   int NOT NULL,
  transaction_product_id  int NOT NULL,
  total_quantity_returned decimal(10,3) DEFAULT 0,
  total_vat               decimal(12,2) DEFAULT 0,
  total_amount            decimal(12,2) DEFAULT 0,
  status                  varchar(20) DEFAULT 'Active',
  created_by              int,
  created_at              timestamp DEFAULT NOW()
);
```

#### exchange_transactions
Records for product exchanges.
```sql
CREATE TABLE exchange_transactions (
  id                        int PRIMARY KEY AUTO_INCREMENT,
  customer_id               int,
  selling_transaction_id    int NOT NULL,
  shift_id                  int,
  total_exchanged_amount    decimal(12,2) DEFAULT 0,
  total_exchanged_vat       decimal(12,2) DEFAULT 0,
  product_exchanged_quantity decimal(10,3) DEFAULT 0,
  exchange_time             timestamp DEFAULT NOW(),
  status                    varchar(20) DEFAULT 'Active',
  created_by                int,
  created_at                timestamp DEFAULT NOW()
);
```

#### customer_payments
Records customer payments and account credits.
```sql
CREATE TABLE customer_payments (
  id              int PRIMARY KEY AUTO_INCREMENT,
  customer_id     int,
  shift_id        int,
  amount_paid     decimal(12,2) DEFAULT 0,
  cheque_number   varchar(50),
  cheque_date     date,
  cheque_bank_name varchar(100),
  payment_time    timestamp DEFAULT NOW(),
  is_paid         boolean DEFAULT true,
  payment_type    varchar(20),
  ref_payment_id  int,
  status          varchar(20) DEFAULT 'Active',
  created_by      int,
  created_at      timestamp DEFAULT NOW()
);
```

---

### 💳 Financial Management

#### payment_types
Payment method definitions.
```sql
CREATE TABLE payment_types (
  id int PRIMARY KEY AUTO_INCREMENT,
  business_id int NOT NULL,
  name varchar(255) NOT NULL,
  payment_code varchar(50) NOT NULL,
  status boolean DEFAULT true NOT NULL,
  change_allowed boolean DEFAULT false,
  customer_required boolean DEFAULT false,
  mark_transaction_as_paid boolean DEFAULT true,
  shortcut_key varchar(10),
  is_refundable boolean DEFAULT true,
  is_split_allowed boolean DEFAULT true,
  created_by int,
  created_at datetime DEFAULT CURRENT_TIMESTAMP,
  updated_by int,
  updated_at datetime DEFAULT CURRENT_TIMESTAMP,
  deleted_by int,
  deleted_at datetime,
  UNIQUE KEY unique_business_payment_code (business_id, payment_code)
);
```

#### payment_transaction
Records payment transactions.
```sql
CREATE TABLE payment_transaction (
  id int PRIMARY KEY AUTO_INCREMENT,
  sales_id int,
  payment_method_id  int NOT NULL,
  amount decimal(12,2) NOT NULL,
  transaction_reference varchar(100),
  notes text,
  created_at timestamp DEFAULT NOW()
);
```

#### tax_types
Defines tax types and rates.
```sql
CREATE TABLE tax_types (
  id                  bigint PRIMARY KEY AUTO_INCREMENT,
  name                varchar(100) NOT NULL,
  description         text,
  value               decimal(10,4) NOT NULL,           -- Tax rate or fixed value
  included_in_price   boolean DEFAULT false,
  is_percentage       boolean DEFAULT true,
  applies_to_buying   boolean DEFAULT false,
  applies_to_selling  boolean DEFAULT true,
  status              boolean DEFAULT true,
  created_by          bigint,
  created_at          datetime DEFAULT CURRENT_TIMESTAMP,
  updated_by          bigint,
  updated_at          datetime,
  deleted_by          bigint,
  deleted_at          datetime
);
```

#### product_taxes
Links products to applicable tax types.
```sql
CREATE TABLE product_taxes (
  id          bigint PRIMARY KEY AUTO_INCREMENT,
  product_id  bigint NOT NULL,
  tax_type_id bigint NOT NULL
);
```

#### service_charges
Defines service charges that can be applied to transactions.
```sql
CREATE TABLE service_charges (
  id              int PRIMARY KEY AUTO_INCREMENT,
  name            varchar(100),                         -- e.g., Service Fee, Delivery Fee
  description     text,                                 -- Optional: Explanation of the charge
  charge_type     enum('flat', 'percentage'),           -- Flat amount or percentage
  charge_value    decimal(10,2),                        -- The amount (e.g., 50.00 or 5.00%)
  charge_value_arabic    decimal(10,2),
  apply_to        enum('invoice', 'product', 'category', 'order'), -- Where it applies
  is_taxable      boolean DEFAULT false,                -- Whether this service charge is taxable
  is_active       boolean DEFAULT true,
  created_by varchar(255),
  created_at timestamp,
  updated_by varchar(255),
  updated_at timestamp,
  deleted_at timestamp,
  deleted_by varchar(255)
);
```

#### service_charge_types
Master table for service charge types.
```sql
CREATE TABLE service_charge_types (
  id   bigint PRIMARY KEY AUTO_INCREMENT,
  service_charge_type_code varchar(20) NOT NULL UNIQUE, -- Short code (e.g., 'SC_TAX', 'SC_DEL')
  name                     varchar(100) NOT NULL,       -- Descriptive name (e.g., 'Delivery Charge', 'Service Tax')
  description              text,                         -- Optional extra details
  calculation_method       varchar(20) DEFAULT 'PERCENT', -- PERCENT or FIXED
  rate_value               decimal(10,2),                -- Value (e.g., 5.00 for 5% or fixed amount)
  status                   boolean DEFAULT true,         -- Active/inactive flag
  created_by               bigint,                       -- User ID who created
  created_at               datetime DEFAULT CURRENT_TIMESTAMP, -- Creation time
  updated_by               bigint,                       -- User ID who last updated
  updated_at               datetime,                     -- Last update time
  deleted_by               bigint,                       -- User ID who deleted
  deleted_at               datetime                      -- Soft delete timestamp
);
```

#### service_charge_types_translations
Multilingual support for service charge types.
```sql
CREATE TABLE service_charge_types_translations (
  id                      bigint PRIMARY KEY AUTO_INCREMENT,
  language_id             bigint NOT NULL,              -- FK to languages table
  service_charge_type_id  bigint NOT NULL,              -- FK to main service charge type
  name                    varchar(100) NOT NULL,        -- Translated name
  status                  boolean DEFAULT true,         -- Active/inactive translation
  created_by              bigint,                       -- User who created this translation
  created_at              datetime DEFAULT CURRENT_TIMESTAMP, -- Creation timestamp
  updated_by              bigint,                       -- User who last updated
  updated_at              datetime,                     -- Last update timestamp
  deleted_by              bigint,                       -- User who deleted
  deleted_at              datetime                      -- Soft delete timestamp
);
```

#### service_charge_options
Configuration options for service charges.
```sql
CREATE TABLE service_charge_options (
  service_charge_option_id bigint PRIMARY KEY AUTO_INCREMENT,
  service_charge_type_id   bigint NOT NULL,             -- FK to main service charge type
  cost                     decimal(10,2),               -- Internal cost
  language_id              bigint NOT NULL,             -- Default language for this option entry
  name                     varchar(100) NOT NULL,       -- Default name
  price                    decimal(10,2),               -- Selling price for this option
  status                   boolean DEFAULT true,        -- Active/inactive flag
  created_by               bigint,                      -- Created by user
  created_at               datetime DEFAULT CURRENT_TIMESTAMP, -- Creation time
  updated_by               bigint,                      -- Last updated by user
  updated_at               datetime,                    -- Last update time
  deleted_by               bigint,                      -- Deleted by user
  deleted_at               datetime                     -- Soft delete time
);
```

#### service_charge_options_translations
Multilingual support for service charge options.
```sql
CREATE TABLE service_charge_options_translations (
  id                        bigint PRIMARY KEY AUTO_INCREMENT,
  language_id               bigint NOT NULL,            -- FK to languages
  service_charge_option_id  bigint NOT NULL,            -- FK to main option
  name                      varchar(100) NOT NULL,      -- Translated name
  status                    boolean DEFAULT true,       -- Active/inactive
  created_by                bigint,                     -- Created by user
  created_at                datetime DEFAULT CURRENT_TIMESTAMP, -- Creation time
  updated_by                bigint,                     -- Last updated by user
  updated_at                datetime,                   -- Last update time
  deleted_by                bigint,                     -- Deleted by user
  deleted_at                datetime                    -- Soft delete time
);
```

---

### ⏰ Shift & Cash Management

#### shift_types
Template shifts for locations.
```sql
CREATE TABLE shift_types (
  shift_type_id bigint PRIMARY KEY AUTO_INCREMENT,
  shift_name varchar(100) NOT NULL,
  shop_location_id bigint NOT NULL,
  start_time time NOT NULL,
  end_time time NOT NULL,
  status boolean DEFAULT true,                          -- Active/Inactive
  created_by bigint NOT NULL,
  created_at timestamp DEFAULT NOW(),
  updated_by bigint,
  updated_at timestamp DEFAULT NOW(),
  deleted_at timestamp,
  deleted_by bigint
);
```

#### shifts
Actual shift instances with financial tracking.
```sql
CREATE TABLE shifts (
  shift_id bigint PRIMARY KEY AUTO_INCREMENT,
  shift_type_id bigint,
  cashbox_id bigint NOT NULL,
  start_balance decimal(12,2) DEFAULT 0.00,
  end_balance decimal(12,2) DEFAULT 0.00,
  open_date_time timestamp NOT NULL,
  close_date_time timestamp,
  status varchar(20) DEFAULT 'Scheduled',               -- Scheduled/In-Progress/Completed/Closed
  created_by bigint NOT NULL,
  created_at timestamp DEFAULT NOW(),
  updated_by bigint,
  updated_at timestamp DEFAULT NOW(),
  deleted_at timestamp,
  deleted_by bigint
);
```

#### shift_user
Assigns users to specific shifts.
```sql
CREATE TABLE shift_user (
  shift_user_id bigint PRIMARY KEY AUTO_INCREMENT,
  shift_id bigint NOT NULL,
  user_id bigint NOT NULL,
  status varchar(20) DEFAULT 'Active',                  -- Active/Replaced/Cancelled
  created_by bigint NOT NULL,
  created_at timestamp DEFAULT NOW(),
  updated_by bigint,
  updated_at timestamp DEFAULT NOW(),
  deleted_at timestamp,
  deleted_by bigint,
  UNIQUE KEY unique_shift_user (shift_id, user_id)
);
```

#### shift_transactions
Detailed shift financial transactions.
```sql
CREATE TABLE shift_transactions (
  id bigint PRIMARY KEY AUTO_INCREMENT,
  shift_id bigint NOT NULL,
  transaction_type varchar(50) NOT NULL,                -- Open/Close/Adjustment
  amount decimal(12,2) NOT NULL,
  notes text,
  created_by bigint NOT NULL,
  created_at timestamp DEFAULT NOW()
);
```

#### cash_box
Physical cash boxes at each location.
```sql
CREATE TABLE cash_box (
  cash_box_id int PRIMARY KEY AUTO_INCREMENT,
  shop_location_id int NOT NULL,
  shift_id int NOT NULL,
  cash_box_name varchar(100) NOT NULL,
  status varchar(20) DEFAULT 'Active',                  -- Open/Closed/Inactive
  created_by int NOT NULL,
  created_at timestamp DEFAULT NOW(),
  updated_by int,
  updated_at timestamp DEFAULT NOW(),
  deleted_at timestamp,
  deleted_by int,
  INDEX idx_shop_location_id (shop_location_id),
  INDEX idx_shift_id (shift_id)
);
```

#### cash_in_out
Records all cash movements in/out of boxes.
```sql
CREATE TABLE cash_in_out (
  cash_in_id int PRIMARY KEY AUTO_INCREMENT,
  cash_box_id int NOT NULL,
  payment_id int NOT NULL,
  reason varchar(255),
  amount_out decimal(12,2) DEFAULT 0.00,
  amount_in decimal(12,2) DEFAULT 0.00,
  note text,
  status varchar(20) DEFAULT 'Completed',               -- Completed/Pending/Cancelled
  created_by int NOT NULL,
  created_at timestamp DEFAULT NOW(),
  updated_by int,
  updated_at timestamp DEFAULT NOW(),
  deleted_at timestamp,
  deleted_by int,
  INDEX idx_cash_box_id (cash_box_id),
  INDEX idx_payment_id (payment_id),
  INDEX idx_created_at (created_at)
);
```

#### cash_box_balance
Calculated current balance for each cash box.
```sql
CREATE TABLE cash_box_balance (
  cash_box_id int PRIMARY KEY,
  current_balance decimal(12,2) NOT NULL,
  last_updated timestamp DEFAULT NOW()
);
```

---

### 🏬 Shop & Location Management

#### shop
Main shop/store information table.
```sql
CREATE TABLE shop (
  shop_id int PRIMARY KEY AUTO_INCREMENT,
  company_id int NOT NULL,
  industry_type_id int,
  name varchar(100) NOT NULL,
  pos_id varchar(50),                                   -- POS system identifier
  pos_name varchar(100),
  number_of_locations_allowed int DEFAULT 1,
  status varchar(20) DEFAULT 'Active',
  created_by int,
  created_at timestamp DEFAULT NOW(),
  updated_by int,
  updated_at timestamp DEFAULT NOW(),
  deleted_at timestamp,
  deleted_by int
);
```

#### stores
Store location details.
```sql
CREATE TABLE stores (
  id int PRIMARY KEY AUTO_INCREMENT,
  name varchar(100) NOT NULL,
  address varchar(200),
  phone_number varchar(50),
  email varchar(100),
  manager_name varchar(100),
  is_active boolean DEFAULT true,
  is_default boolean DEFAULT false,
  created_at datetime DEFAULT CURRENT_TIMESTAMP NOT NULL,
  updated_at datetime DEFAULT CURRENT_TIMESTAMP NOT NULL
);
```

#### shop_locations
Physical locations for shops (stores, warehouses, etc.).
```sql
CREATE TABLE shop_locations (
  id int PRIMARY KEY AUTO_INCREMENT,
  shop_id int NOT NULL,
  location_type varchar(50) NOT NULL,                   -- Retail/Warehouse/etc.
  location_name varchar(100) NOT NULL,
  manager_id int,
  address_line1 varchar(255) NOT NULL,
  address_line2 varchar(255),
  building varchar(100),
  area varchar(100),
  po_box varchar(20),
  city varchar(100),
  state_id int,
  country_id int,
  landline_number varchar(20),
  mobile_number varchar(20),
  location_latitude decimal(10,8),
  location_longitude decimal(11,8),
  can_sell boolean DEFAULT true,
  language_id int,
  status varchar(20) DEFAULT 'Active',
  created_by int,
  created_at timestamp DEFAULT NOW(),
  updated_by int,
  updated_at timestamp DEFAULT NOW(),
  deleted_at timestamp,
  deleted_by int,
  INDEX idx_shop_id (shop_id),
  INDEX idx_location_type (location_type)
);
```

#### shop_location_translation_ar
Arabic translations for location information.
```sql
CREATE TABLE shop_location_translation_ar (
  id int PRIMARY KEY AUTO_INCREMENT,
  shop_location_id int NOT NULL,
  language_id int NOT NULL,
  name_ar varchar(100),
  address_line1_ar varchar(255),
  address_line2_ar varchar(255),
  building_ar varchar(100),
  area_ar varchar(100),
  status varchar(20) DEFAULT 'Active',
  created_by int,
  created_at timestamp DEFAULT NOW(),
  updated_by int,
  updated_at timestamp DEFAULT NOW(),
  deleted_at timestamp,
  deleted_by int,
  UNIQUE KEY unique_shop_location_language (shop_location_id, language_id)
);
```

#### shop_users
Assigns users to specific shops.
```sql
CREATE TABLE shop_users (
  shop_users_id int PRIMARY KEY AUTO_INCREMENT,
  shop_id int NOT NULL,
  user_id int NOT NULL,
  status varchar(20) DEFAULT 'Active',
  created_by int,
  created_at timestamp DEFAULT NOW(),
  updated_by int,
  updated_at timestamp DEFAULT NOW(),
  deleted_at timestamp,
  deleted_by int,
  UNIQUE KEY unique_shop_user (shop_id, user_id)
);
```

#### shop_type
Defines different types of shops/stores.
```sql
CREATE TABLE shop_type (
  shop_type_id int PRIMARY KEY AUTO_INCREMENT,
  name varchar(100) NOT NULL,
  default_selling_price_type_id int,
  start_time time,                                      -- Default opening time
  end_time time,                                        -- Default closing time
  status varchar(20) DEFAULT 'Active',
  created_by int,
  created_at timestamp DEFAULT NOW(),
  updated_by int,
  updated_at timestamp DEFAULT NOW(),
  deleted_at timestamp,
  deleted_by int
);
```

---

### 🍽️ Restaurant Features

#### restaurant_tables
Table management.
```sql
CREATE TABLE restaurant_tables (
  id int PRIMARY KEY AUTO_INCREMENT,
  table_number varchar(10),
  capacity int,
  status enum('available','reserved','occupied','cleaning') DEFAULT 'available',
  location varchar(50),
  created_at timestamp DEFAULT CURRENT_TIMESTAMP
);
```

#### reservation
Manages table reservations for restaurants.
```sql
CREATE TABLE reservation (
  id int PRIMARY KEY AUTO_INCREMENT,
  deleted int,
  customer_id int,                                      -- Foreign key to Customer
  table_number int,                                     -- Table number assigned/reserved
  number_of_persons int,                                -- Total guests for reservation
  reservation_date date,                                -- Date of reservation
  reservation_time time,                                -- Time of reservation (24-hour)
  deposit_fee decimal(10, 2),                           -- Advance payment
  payment_method  int NOT NULL,
  status enum('confirmed', 'waiting', 'cancelled') DEFAULT 'waiting', -- Reservation status
  notes text,                                           -- Optional notes or special requests
  created_at timestamp DEFAULT NOW()
);
```

---

### 📊 Unit of Measure (UoM) Management

#### uom
Master table for units of measurement with conversion support.
```sql
CREATE TABLE uom (
  id bigint PRIMARY KEY,
  name varchar(50),
  abbreviation varchar(10),
  base_uom_id bigint,
  conversion_factor decimal(10,4),
  is_active boolean,
  created_at datetime,
  updated_at datetime
);
```

#### unit_options
Master table for all unit types (kg, liter, etc.).
```sql
CREATE TABLE unit_options (
  units_options_id int PRIMARY KEY AUTO_INCREMENT,
  name varchar(50) NOT NULL,
  description varchar(255),
  category_title varchar(50),
  type varchar(20) NOT NULL,                            -- 'Base' or 'Derived'
  status varchar(20) DEFAULT 'Active',
  created_by int NOT NULL,
  created_at timestamp DEFAULT NOW(),
  updated_by int,
  updated_at timestamp DEFAULT NOW(),
  deleted_at timestamp,
  deleted_by int
);
```

#### product_unit
Defines packaging units for products.
```sql
CREATE TABLE product_unit (
  id              int PRIMARY KEY AUTO_INCREMENT,
  product_id      int NOT NULL,
  unit_name       varchar(50) NOT NULL,
  unit_symbol     varchar(10) NOT NULL,
  parent_unit_id  int,
  qty_in_parent   int NOT NULL DEFAULT 1,
  is_base         boolean NOT NULL DEFAULT false,
  created_at      timestamp NOT NULL DEFAULT NOW()
);
```

#### unit_prices
Pricing information by unit.
```sql
CREATE TABLE unit_prices (
  id              int PRIMARY KEY AUTO_INCREMENT,
  product_id      int NOT NULL,
  unit_id         int NOT NULL,
  color           varchar(50),
  cost            decimal(10,2) NOT NULL,
  selling_price_id   int NOT NULL,
  price_type      varchar(50) NOT NULL DEFAULT 'Retail',
  discount_allowed boolean NOT NULL DEFAULT false,
  created_at      timestamp NOT NULL DEFAULT NOW(),
  updated_at      timestamp NOT NULL DEFAULT NOW()
);
```

#### unit_option_conversion
Defines conversion rules between product variants.
```sql
CREATE TABLE unit_option_conversion (
  id int PRIMARY KEY AUTO_INCREMENT,
  unit_option_id int NOT NULL,
  unit_value_from_id int NOT NULL,
  unit_value_to_id int NOT NULL,
  unit_conversion_rate decimal(12,6) NOT NULL,
  status varchar(20) DEFAULT 'Active',
  created_by int NOT NULL,
  created_at timestamp DEFAULT NOW(),
  updated_by int,
  updated_at timestamp DEFAULT NOW(),
  deleted_at timestamp,
  deleted_by int,
  UNIQUE KEY unique_unit_conversion (unit_value_from_id, unit_value_to_id),
  INDEX idx_unit_option_id (unit_option_id)
);
```

---

### 📅 Programs & Subscriptions

#### Programs
Defines subscription programs for customers.
```sql
CREATE TABLE Programs (
  Program_ID int PRIMARY KEY AUTO_INCREMENT,
  vat decimal(10,2),
  is_percentage_discount boolean,
  discount_value decimal(10,2),
  discount_max_value decimal(10,2),
  discount_note varchar(255),
  total_applied_vat decimal(10,2),
  total_applied_discounts_value decimal(10,2),
  Name varchar(150),
  Description text,
  Start_Date date,
  End_Date date,
  Program_Price decimal(10,2),
  Number_of_Meals int,
  Status varchar(20),
  created_by int,
  created_at timestamp,
  Updated_by int,
  updated_at timestamp,
  deleted_at timestamp,
  deleted_by int
);
```

#### Program_Translations
Multilingual support for program information.
```sql
CREATE TABLE Program_Translations (
  id int PRIMARY KEY AUTO_INCREMENT,
  Program_id int,
  language_id int,
  name varchar(150),
  description text,
  Status varchar(20),
  created_by int,
  created_at timestamp,
  Updated_by int,
  updated_at timestamp,
  deleted_at timestamp,
  deleted_by int
);
```

#### Program_Meals
Defines meals included in programs.
```sql
CREATE TABLE Program_Meals (
  Program_Meals_ID int PRIMARY KEY AUTO_INCREMENT,
  Program_ID int,
  Name varchar(150),
  Time time,
  Status varchar(20),
  created_by int,
  created_at timestamp,
  Updated_by int,
  updated_at timestamp,
  deleted_at timestamp,
  deleted_by int
);
```

#### Program_Products
Products included in subscription programs.
```sql
CREATE TABLE Program_Products (
  id int PRIMARY KEY AUTO_INCREMENT,
  program_id int,
  product_id int,
  product_unit_id int,
  quantity int,
  price decimal(10,2),
  vat decimal(10,2),
  is_percentage_discount boolean,
  discount_value decimal(10,2),
  discount_max_value decimal(10,2),
  discount_note varchar(255),
  Status varchar(20),
  created_by int,
  created_at timestamp,
  Updated_by int,
  updated_at timestamp,
  deleted_at timestamp,
  deleted_by int
);
```

#### Program_Subscriptions
Customer subscriptions to programs.
```sql
CREATE TABLE Program_Subscriptions (
  id int PRIMARY KEY AUTO_INCREMENT,
  program_id int,
  Customer_ID int,
  Status varchar(20),
  created_by int,
  created_at timestamp,
  Updated_by int,
  updated_at timestamp,
  deleted_at timestamp,
  deleted_by int
);
```

#### Program_Order_Delivery
Tracks delivery of program orders.
```sql
CREATE TABLE Program_Order_Delivery (
  id int PRIMARY KEY AUTO_INCREMENT,
  program_id int,
  customer_id int,
  delivery_status varchar(20),
  send_time datetime,
  Status varchar(20),
  created_by int,
  created_at timestamp,
  Updated_by int,
  updated_at timestamp,
  deleted_at timestamp,
  deleted_by int
);
```

---

### 📈 Additional Features

#### product_groups
Product grouping for bundles.
```sql
CREATE TABLE product_groups (
  id int PRIMARY KEY AUTO_INCREMENT,
  name varchar(255) NOT NULL,
  name_ar varchar(255),
  description text,
  description_ar text,
  discount_id int,
  tax_type_id int,
  price_type_id bigint,
  sku_prefix varchar(50),
  status varchar(50) DEFAULT 'Active',
  created_date datetime DEFAULT CURRENT_TIMESTAMP,
  modified_date datetime,
  created_by int,
  modified_by int,
  is_deleted boolean DEFAULT false,
  deleted_date datetime,
  deleted_by int
);
```

#### selling_price_types
Defines different price types (Retail, Wholesale, etc.).
```sql
CREATE TABLE selling_price_types (
  id bigint PRIMARY KEY AUTO_INCREMENT,
  type_name              varchar(100) NOT NULL,        -- Name (e.g., 'Retail', 'Wholesale', 'Member')
  arabic_name             varchar(100) NOT NULL,        -- Name (e.g., 'Retail', 'Wholesale', 'Member')
  description            text,                          -- Optional details about price type
  status                 boolean DEFAULT true,          -- Active/inactive flag
  created_by             bigint,                        -- User ID who created
  created_at             datetime DEFAULT CURRENT_TIMESTAMP, -- Creation time
  updated_by             bigint,                        -- User ID who last updated
  updated_at             datetime,                      -- Last update time
  deleted_by             bigint,                        -- User ID who deleted
  deleted_at             datetime                       -- Soft delete timestamp
);
```

#### Tranasaction_types
Defines types of transactions.
```sql
CREATE TABLE Tranasaction_types (
  id            int PRIMARY KEY AUTO_INCREMENT,
  type_name     varchar(255) NOT NULL,                 -- Type Name
  type_name_ar  varchar(255) NOT NULL,                 -- Type Name AR
  description   text,                                  -- Description
  status        varchar(50),                           -- Status
  created_by    bigint,                                -- FK to users.id (optional)
  created_at    timestamp DEFAULT NOW(),
  updated_by    bigint,                                -- FK to users.id (optional)
  updated_at    timestamp DEFAULT NOW(),
  deleted_at    timestamp,                             -- nullable (soft delete)
  deleted_by    bigint                                 -- FK to users.id (optional)
);
```

---

## 🔗 Key Relationships

### Core Entity Relationships
```
Companies (1) ──→ (N) Shops ──→ (N) Shop_Locations
    │                              │
    └──→ (N) Users ←──────────────┘
    
Products (1) ──→ (N) Product_Info
    │
    ├──→ (N) Product_Variants
    ├──→ (N) Product_Barcodes
    ├──→ (N) Product_Images
    └──→ (N) Stock_Movement

Customers (1) ──→ (N) Transactions ──→ (N) Transaction_Products
    │                    │
    └──→ (N) Reservations └──→ (N) Payment_Transactions

Suppliers (1) ──→ (N) Supplier_Purchase ──→ (N) Goods_Received
```

### Multi-language Support
```
Language (1) ──→ (N) Label_Translation
    │
    ├──→ (N) Category_Translation
    ├──→ (N) Product_Translation
    ├──→ (N) Supplier_Translation
    └──→ (N) Customer_Address_Translation
```

---

## 🛠️ Technical Features

### Audit Trail
- All major tables include audit fields: `created_by`, `created_at`, `updated_by`, `updated_at`, `deleted_by`, `deleted_at`
- Comprehensive activity logging in `activity_logs` table
- Soft delete support across all entities

### Multi-language Support
- Dedicated translation tables for all user-facing content
- RTL (Right-to-Left) language support
- Language-specific formatting and display options

### Multi-tenant Architecture
- Company-based data isolation
- Shop-level user access control
- Location-based inventory management

### Financial Controls
- Multiple currency support
- Flexible tax calculation system
- Comprehensive discount and promotion engine
- Advanced pricing models (retail, wholesale, member)

### Inventory Management
- Batch tracking with expiry dates
- Multi-location stock management
- Automated stock movement tracking
- Comprehensive transfer and adjustment workflows

---

## 📊 Performance Considerations

### Indexing Strategy
- Primary keys on all tables
- Foreign key constraints for referential integrity
- Composite indexes on frequently queried combinations
- Unique constraints on business keys (SKU, barcode, email)

### Partitioning Recommendations
- Consider partitioning large transaction tables by date
- Archive old activity logs periodically
- Implement read replicas for reporting queries

---

## 🔒 Security Features

### Access Control
- Role-based permission system
- Hierarchical permission structure
- Temporary permission overrides
- Industry-specific access controls

### Data Protection
- Encrypted sensitive fields (passwords, financial data)
- Audit trail for all data modifications
- Soft delete for data recovery
- License-based feature access control

---

## 🚀 Implementation Notes

### Database Engine Requirements
- MySQL 8.0+ or MariaDB 10.5+
- InnoDB storage engine for ACID compliance
- UTF-8 character set for multilingual support
- JSON data type support for flexible configurations

### Migration Strategy
- Use Entity Framework Core migrations
- Implement database seeding for master data
- Version control for schema changes
- Backup and rollback procedures

### Monitoring & Maintenance
- Regular index optimization
- Query performance monitoring
- Automated backup schedules
- Data archival policies for historical data