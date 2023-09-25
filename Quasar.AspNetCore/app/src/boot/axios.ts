import {boot} from 'quasar/wrappers';
import axios, {AxiosInstance} from 'axios';
//import { globalRouter } from "../globalRouter";
//import {useRouter} from 'vue-router';
//import router from "../router";

declare module '@vue/runtime-core' {
  interface ComponentCustomProperties {
    $axios: AxiosInstance;
    $api: AxiosInstance;
  }
}

// Be careful when using SSR for cross-request state pollution
// due to creating a Singleton instance here;
// If any client changes this (global) instance, it might be a
// good idea to move this instance creation inside of the
// "export default () => {}" function below (which runs individually
// for each client)
const api = axios.create({
  // maxRedirects: 0,
  // validateStatus: function (status: number) {
  //   switch (status) {
  //     case 302:
  //       return true;
  //     default:
  //       return false;
  //   }
  // }
});


//Added so that ASP.NET returns a 401 rather than a 302 redirect
api.defaults.headers.common['X-Requested-With'] = 'XMLHttpRequest';

api.interceptors.response.use(null, async error => {


  //const router = useRouter();

  let path = '/error';
  switch (error.response.status) {
    case 302:
    case 401:
      path = '/api/login?redirecturi=/';
      break;
    case 404:
      path = '/404';
      break;
    case 403:
      path = '/403';
      break;
  }
  window.location.href = path;
  //router?.push(path);
  //this.$router.push(path);
  return Promise.reject(error);
});

export default boot(({app}) => {
  // for use inside Vue files (Options API) through this.$axios and this.$api

  app.config.globalProperties.$axios = axios;
  // ^ ^ ^ this will allow you to use this.$axios (for Vue Options API form)
  //       so you won't necessarily have to import axios in each vue file

  app.config.globalProperties.$api = api;
  // ^ ^ ^ this will allow you to use this.$api (for Vue Options API form)
  //       so you can easily perform requests against your app's API
});

export {api};
