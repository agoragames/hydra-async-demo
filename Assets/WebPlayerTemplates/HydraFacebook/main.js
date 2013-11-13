window.fbAsyncInit = function() {
  FB.init({
    appId: '314010021960072',
    status: true, // check login status
    cookie: true, // enable cookies to allow the server to access the session
    oauth: true, // enable OAuth 2.0
    xfbml: false // dont parse XFBML
  });
};

function initHydraSdk(unityName) {
  console.log("Hydra SDK is initialize");
  window.unityName = unityName;
}

function LinkFacebook() {
   FB.login(function(response){
        console.log("Logged in to facebook");
        getFBAccessToken();
      }, {scope: 'email'});
}

function getFBAccessToken() {
  console.log("Sending Facebook access token to Hydra");
  var token = FB.getAccessToken()
  unityObject.getUnity().SendMessage(window.unityName, "AuthenticateWithFacebookAccessToken", token);
}

//Load the Facebook JS SDK
(function(d) {
  var js, id = 'facebook-jssdk';
  if (d.getElementById(id)) {
    return;
  }
  js = d.createElement('script');
  js.id = id;
  js.async = true;
  js.src = "//connect.facebook.net/en_US/all.js";
  d.getElementsByTagName('head')[0].appendChild(js);
}(document));