-- Migration: Add stock_ledger table
-- Created: 2025-11-06
-- Description: Creates stock_ledger table to track all stock movements with running balance

CREATE TABLE IF NOT EXISTS stock_ledger (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    product_id INTEGER NOT NULL,
    unit_id INTEGER NOT NULL,
    movement_type INTEGER NOT NULL,
    qty DECIMAL(10,2) NOT NULL,
    balance DECIMAL(10,2) NOT NULL,
    location VARCHAR(200) NULL,
    reference_type INTEGER NULL,
    reference_id INTEGER NULL,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    note TEXT NULL,
    
    CONSTRAINT FK_stock_ledger_product FOREIGN KEY (product_id) 
        REFERENCES products(id) ON DELETE RESTRICT,
    CONSTRAINT FK_stock_ledger_unit FOREIGN KEY (unit_id) 
        REFERENCES product_units(id) ON DELETE RESTRICT
);

-- Create indexes for better query performance
CREATE INDEX IF NOT EXISTS IX_stock_ledger_product_id 
    ON stock_ledger(product_id);

CREATE INDEX IF NOT EXISTS IX_stock_ledger_product_created 
    ON stock_ledger(product_id, created_at);

CREATE INDEX IF NOT EXISTS IX_stock_ledger_movement_type 
    ON stock_ledger(movement_type);

CREATE INDEX IF NOT EXISTS IX_stock_ledger_reference_type 
    ON stock_ledger(reference_type);

-- Verify table creation
SELECT 'stock_ledger table created successfully' AS status;
