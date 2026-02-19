<template>
  <div class="products-page">
    <div class="page-header">
      <div>
        <h1>Product Inventory</h1>
        <p class="page-subtitle">Manage your inventory items</p>
      </div>
      <button class="btn btn-primary" @click="openCreateModal">+ Add Product</button>
    </div>

    <!-- Error Alert -->
    <div v-if="error" class="alert alert-error">
      ⚠️ {{ error }}
      <button class="alert-close" @click="error = ''">×</button>
    </div>

    <!-- Loading -->
    <div v-if="loading" class="loading-state">
      <div class="spinner"></div>
      <p>Loading products...</p>
    </div>

    <!-- Empty State -->
    <div v-else-if="products.length === 0" class="empty-state">
      <div class="empty-icon">📦</div>
      <h3>No products yet</h3>
      <p>Click "Add Product" to create your first inventory item.</p>
    </div>

    <!-- Products Table -->
    <div v-else class="table-wrapper">
      <table class="products-table">
        <thead>
          <tr>
            <th>#</th>
            <th>Name</th>
            <th>Category</th>
            <th>Price</th>
            <th>Qty</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="product in products" :key="product.id">
            <td class="id-cell">{{ product.id }}</td>
            <td>
              <div class="product-name">{{ product.name }}</div>
              <div class="product-desc">{{ product.description }}</div>
            </td>
            <td><span class="badge">{{ product.category }}</span></td>
            <td class="price-cell">${{ product.price.toFixed(2) }}</td>
            <td>
              <span :class="['qty-badge', product.quantity < 10 ? 'low' : 'ok']">
                {{ product.quantity }}
              </span>
            </td>
            <td class="actions-cell">
              <button class="icon-btn edit" @click="openEditModal(product)" title="Edit">✏️</button>
              <button class="icon-btn delete" @click="deleteProduct(product.id)" title="Delete">🗑️</button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Modal -->
    <Teleport to="body">
      <div v-if="showModal" class="modal-overlay" @click.self="closeModal">
        <div class="modal">
          <div class="modal-header">
            <h2>{{ editingProduct ? 'Edit Product' : 'Add Product' }}</h2>
            <button class="modal-close" @click="closeModal">×</button>
          </div>
          <form class="modal-form" @submit.prevent="saveProduct">
            <div class="form-row">
              <div class="form-group">
                <label>Name *</label>
                <input v-model="form.name" type="text" required placeholder="Product name" />
              </div>
              <div class="form-group">
                <label>Category *</label>
                <input v-model="form.category" type="text" required placeholder="e.g. Electronics" />
              </div>
            </div>
            <div class="form-group">
              <label>Description</label>
              <textarea v-model="form.description" rows="2" placeholder="Brief description"></textarea>
            </div>
            <div class="form-row">
              <div class="form-group">
                <label>Price *</label>
                <input v-model.number="form.price" type="number" min="0" step="0.01" required />
              </div>
              <div class="form-group">
                <label>Quantity *</label>
                <input v-model.number="form.quantity" type="number" min="0" required />
              </div>
            </div>
            <div class="modal-actions">
              <button type="button" class="btn btn-ghost" @click="closeModal">Cancel</button>
              <button type="submit" class="btn btn-primary" :disabled="saving">
                {{ saving ? 'Saving...' : editingProduct ? 'Update' : 'Create' }}
              </button>
            </div>
          </form>
        </div>
      </div>
    </Teleport>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { productsApi, type Product, type CreateProductPayload } from '@/api/productsApi'

const products = ref<Product[]>([])
const loading = ref(false)
const saving = ref(false)
const error = ref('')
const showModal = ref(false)
const editingProduct = ref<Product | null>(null)

const emptyForm = (): CreateProductPayload => ({
  name: '', description: '', price: 0, quantity: 0, category: ''
})
const form = ref<CreateProductPayload>(emptyForm())

async function loadProducts() {
  loading.value = true
  error.value = ''
  try {
    products.value = await productsApi.getAll()
  } catch (e: any) {
    error.value = e.response?.data?.title ?? 'Failed to load products'
  } finally {
    loading.value = false
  }
}

function openCreateModal() {
  editingProduct.value = null
  form.value = emptyForm()
  showModal.value = true
}

function openEditModal(product: Product) {
  editingProduct.value = product
  form.value = {
    name: product.name,
    description: product.description,
    price: product.price,
    quantity: product.quantity,
    category: product.category,
  }
  showModal.value = true
}

function closeModal() {
  showModal.value = false
  editingProduct.value = null
  form.value = emptyForm()
}

async function saveProduct() {
  saving.value = true
  error.value = ''
  try {
    if (editingProduct.value) {
      const updated = await productsApi.update(editingProduct.value.id, form.value)
      const idx = products.value.findIndex((p) => p.id === updated.id)
      if (idx !== -1) products.value[idx] = updated
    } else {
      const created = await productsApi.create(form.value)
      products.value.unshift(created)
    }
    closeModal()
  } catch (e: any) {
    error.value = e.response?.data?.title ?? 'Failed to save product'
  } finally {
    saving.value = false
  }
}

async function deleteProduct(id: number) {
  if (!confirm('Delete this product?')) return
  try {
    await productsApi.remove(id)
    products.value = products.value.filter((p) => p.id !== id)
  } catch {
    error.value = 'Failed to delete product'
  }
}

onMounted(loadProducts)
</script>

<style scoped>
.products-page { padding: 0.5rem 0; }

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 2rem;
}

.page-header h1 { font-size: 1.8rem; font-weight: 800; color: #0f172a; }
.page-subtitle { color: #64748b; font-size: 0.9rem; margin-top: 0.3rem; }

.btn {
  padding: 0.6rem 1.3rem;
  border-radius: 8px;
  font-size: 0.9rem;
  font-weight: 600;
  cursor: pointer;
  border: none;
  transition: all 0.2s;
}

.btn-primary { background: linear-gradient(135deg, #6366f1, #8b5cf6); color: #fff; }
.btn-primary:hover { background: linear-gradient(135deg, #4f46e5, #7c3aed); }
.btn-primary:disabled { opacity: 0.6; cursor: not-allowed; }
.btn-ghost { background: #f1f5f9; color: #475569; }
.btn-ghost:hover { background: #e2e8f0; }

.alert {
  padding: 0.85rem 1rem;
  border-radius: 8px;
  margin-bottom: 1.5rem;
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 0.9rem;
}

.alert-error { background: #fef2f2; border: 1px solid #fca5a5; color: #b91c1c; }
.alert-close { background: none; border: none; cursor: pointer; font-size: 1.2rem; color: inherit; }

.loading-state, .empty-state {
  text-align: center;
  padding: 4rem 2rem;
  color: #64748b;
}

.spinner {
  width: 40px; height: 40px;
  border: 3px solid #e2e8f0;
  border-top-color: #6366f1;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
  margin: 0 auto 1rem;
}

@keyframes spin { to { transform: rotate(360deg); } }

.empty-icon { font-size: 3rem; margin-bottom: 1rem; }
.empty-state h3 { font-weight: 700; margin-bottom: 0.5rem; color: #1e293b; }

.table-wrapper {
  background: #fff;
  border-radius: 12px;
  box-shadow: 0 1px 6px rgba(0,0,0,0.06);
  border: 1px solid #e2e8f0;
  overflow: hidden;
}

.products-table { width: 100%; border-collapse: collapse; }

.products-table th {
  background: #f8fafc;
  padding: 0.85rem 1rem;
  text-align: left;
  font-size: 0.8rem;
  font-weight: 700;
  color: #64748b;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  border-bottom: 1px solid #e2e8f0;
}

.products-table td {
  padding: 0.85rem 1rem;
  border-bottom: 1px solid #f1f5f9;
  font-size: 0.9rem;
  color: #374151;
}

.products-table tr:last-child td { border-bottom: none; }
.products-table tbody tr:hover { background: #f8fafc; }

.id-cell { color: #94a3b8; font-size: 0.8rem; }
.product-name { font-weight: 600; color: #0f172a; }
.product-desc { color: #94a3b8; font-size: 0.8rem; margin-top: 2px; }

.badge {
  background: #ede9fe;
  color: #7c3aed;
  padding: 0.25rem 0.6rem;
  border-radius: 20px;
  font-size: 0.75rem;
  font-weight: 600;
}

.price-cell { font-weight: 700; color: #059669; }

.qty-badge {
  padding: 0.25rem 0.6rem;
  border-radius: 20px;
  font-size: 0.8rem;
  font-weight: 700;
}

.qty-badge.ok { background: #dcfce7; color: #166534; }
.qty-badge.low { background: #fef9c3; color: #92400e; }

.actions-cell { white-space: nowrap; }

.icon-btn {
  background: none;
  border: none;
  cursor: pointer;
  font-size: 1rem;
  padding: 0.25rem 0.4rem;
  border-radius: 6px;
  transition: background 0.15s;
  margin-right: 0.2rem;
}

.icon-btn:hover { background: #f1f5f9; }

/* Modal */
.modal-overlay {
  position: fixed; inset: 0;
  background: rgba(0,0,0,0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
  padding: 1rem;
}

.modal {
  background: #fff;
  border-radius: 14px;
  width: 100%;
  max-width: 520px;
  box-shadow: 0 20px 60px rgba(0,0,0,0.2);
  overflow: hidden;
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1.5rem 1.8rem;
  border-bottom: 1px solid #e2e8f0;
}

.modal-header h2 { font-size: 1.2rem; font-weight: 700; }

.modal-close {
  background: none; border: none;
  font-size: 1.5rem; cursor: pointer;
  color: #94a3b8; line-height: 1;
}

.modal-close:hover { color: #374151; }

.modal-form { padding: 1.8rem; }

.form-row { display: grid; grid-template-columns: 1fr 1fr; gap: 1rem; }

.form-group { margin-bottom: 1.1rem; }

.form-group label {
  display: block;
  font-size: 0.85rem;
  font-weight: 600;
  color: #374151;
  margin-bottom: 0.4rem;
}

.form-group input,
.form-group textarea {
  width: 100%;
  padding: 0.6rem 0.8rem;
  border: 1.5px solid #d1d5db;
  border-radius: 8px;
  font-size: 0.95rem;
  outline: none;
  transition: border-color 0.2s;
  font-family: inherit;
  resize: vertical;
}

.form-group input:focus,
.form-group textarea:focus { border-color: #6366f1; }

.modal-actions {
  display: flex;
  justify-content: flex-end;
  gap: 0.75rem;
  margin-top: 0.5rem;
}
</style>
