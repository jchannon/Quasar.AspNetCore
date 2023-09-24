<script setup lang="ts">

import {api} from "boot/axios";

const profile = await api.get<{ name: string, email: string }>("/api/profile")

</script>

<template>
  <q-page class="row items-center justify-evenly">
    <div v-if="profile?.data">Hi there {{ profile.data.name }}!</div>
  </q-page>
</template>

<style scoped>

</style>
