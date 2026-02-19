<template>
  <div></div>
</template>

<script setup lang="ts">
import { onMounted } from 'vue'
import { authService } from '@/services/authService'

onMounted(async () => {
  await authService.silentRenew()
})
</script>
