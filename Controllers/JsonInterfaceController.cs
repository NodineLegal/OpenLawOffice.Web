using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.IO;
using System.Web.Profile;
using System.Data;
using OpenLawOffice.Common.Models.Assets;

namespace OpenLawOffice.Web.Controllers
{
    [HandleError(View = "Errors/Index", Order = 10)]
    [Authorize]
    public class JsonInterfaceController : Controller
    {
        public AccountMembershipService MembershipService { get; set; }

        protected override void Initialize(RequestContext requestContext)
        {
            if (MembershipService == null) { MembershipService = new AccountMembershipService(); }

            base.Initialize(requestContext);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Authenticate()
        {
            dynamic profile;
            Common.Net.Request<Common.Net.AuthPackage> request;
            Common.Net.Response<Guid> response = new Common.Net.Response<Guid>();

            request = Request.InputStream.JsonDeserialize<Common.Net.Request<Common.Net.AuthPackage>>();

            response.RequestReceived = DateTime.Now;

            using (Data.Transaction trans = Data.Transaction.Create(true))
            {
                try
                {
                    Common.Models.Account.Users user = Data.Account.Users.Get(trans, request.Package.Username);
                    profile = ProfileBase.Create(user.Username);

                    // decrypt password
                    Common.Encryption enc = new Common.Encryption();
                    Common.Encryption.Package package;
                    enc.IV = request.Package.IV;
                    if (profile != null && profile.ExternalAppKey != null
                        && !string.IsNullOrEmpty(profile.ExternalAppKey))
                        enc.Key = profile.ExternalAppKey;
                    else
                    {
                        response.Successful = false;
                        response.Package = Guid.Empty;
                    }
                    package = enc.Decrypt(new Common.Encryption.Package()
                    {
                        Input = request.Package.Password
                    });
                    if (string.IsNullOrEmpty(package.Output))
                    {
                        response.Successful = false;
                        response.Package = Guid.Empty;
                    }
                    request.Package.Password = package.Output;

                    string hashFromDb = Security.ClientHashPassword(user.Password);
                    string hashFromWeb = Security.ClientHashPassword(request.Package.Password);

                    if (MembershipService.ValidateUser(request.Package.Username, request.Package.Password))
                    {
                        Common.Models.External.ExternalSession session =
                            Data.External.ExternalSession.Get(trans, request.Package.AppName, request.Package.MachineId, request.Package.Username);
                        user = Data.Account.Users.Get(trans, request.Package.Username);

                        if (session == null)
                        { // create
                            session = Data.External.ExternalSession.Create(trans, new Common.Models.External.ExternalSession()
                            {
                                MachineId = request.Package.MachineId,
                                User = user,
                                AppName = request.Package.AppName
                            });
                        }
                        else
                        { // update
                            session = Data.External.ExternalSession.Update(trans, new Common.Models.External.ExternalSession()
                            {
                                Id = session.Id,
                                MachineId = request.Package.MachineId,
                                User = user,
                                AppName = request.Package.AppName
                            });
                        }

                        response.Successful = true;
                        response.Package = session.Id.Value;
                        trans.Commit();
                    }
                    else
                    {
                        response.Successful = false;
                        response.Package = Guid.Empty;
                        response.Error = "Invalid security credentials.";
                    }
                }
                catch
                {
                    trans.Rollback();
                    response.Successful = false;
                    response.Package = Guid.Empty;
                    response.Error = "Unexpected server error.";
                }
            }

            response.ResponseSent = DateTime.Now;

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult CloseSession()
        {
            Guid token;
            Common.Net.Request<Common.Net.AuthPackage> request;
            Common.Models.External.ExternalSession session;
            Common.Net.Response<bool> response = new Common.Net.Response<bool>();
            
            request = Request.InputStream.JsonDeserialize<Common.Net.Request<Common.Net.AuthPackage>>();
            
            response.RequestReceived = DateTime.Now;

            if ((token = GetToken(Request)) == Guid.Empty)
            {
                response.Successful = false;
                response.Error = "Invalid Token";
                response.Package = false;
                return Json(response, JsonRequestBehavior.AllowGet);
            }

            using (Data.Transaction trans = Data.Transaction.Create(true))
            {
                try
                {
                    if (!VerifyToken(trans, token))
                    {
                        response.Successful = false;
                        response.Error = "Invalid Token";
                        response.Package = false;
                        return Json(response, JsonRequestBehavior.AllowGet);
                    }

                    // Close the session here
                    session = Data.External.ExternalSession.Get(trans, request.Package.AppName, request.Package.MachineId, request.Package.Username);
                    session = Data.External.ExternalSession.Delete(trans, session);

                    trans.Commit();

                    response.Successful = true;
                    response.Package = true;
                }
                catch
                {
                    trans.Rollback();
                    response.Successful = false;
                    response.Package = false;
                    response.Error = "Unexpected server error.";
                }
            }

            response.ResponseSent = DateTime.Now;

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        public ActionResult Matters(string contactFilter, string titleFilter, string caseNumberFilter,
            int? courtTypeFilter, int? courtGeographicalJurisdictionFilter, bool activeFilter = true)
        {
            Guid token;
            Common.Net.Response<List<Common.Models.Matters.Matter>> response
                = new Common.Net.Response<List<Common.Models.Matters.Matter>>();

            response.RequestReceived = DateTime.Now;

            if ((token = GetToken(Request)) == Guid.Empty)
            {
                response.Successful = false;
                response.Error = "Invalid Token";
                response.ResponseSent = DateTime.Now;
                return Json(response, JsonRequestBehavior.AllowGet);
            }

            using (Data.Transaction trans = Data.Transaction.Create(true))
            {
                try
                {
                    if (!VerifyToken(trans, token))
                    {
                        response.Successful = false;
                        response.Error = "Invalid Token";
                        response.ResponseSent = DateTime.Now;
                        return Json(response, JsonRequestBehavior.AllowGet);
                    }

                    response.Successful = true;
                    response.Package = Data.Matters.Matter.List(trans, activeFilter, contactFilter, titleFilter,
                        caseNumberFilter, courtTypeFilter, courtGeographicalJurisdictionFilter);
                }
                catch
                {
                    trans.Rollback();
                    response.Successful = false;
                    response.Package = null;
                    response.Error = "Unexpected server error.";
                }
            }

            response.ResponseSent = DateTime.Now;
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        public ActionResult GetMatter(Guid matterId)
        {
            Guid token;
            Common.Net.Response<Common.Models.Matters.Matter> response
                = new Common.Net.Response<Common.Models.Matters.Matter>();

            response.RequestReceived = DateTime.Now;

            if ((token = GetToken(Request)) == Guid.Empty)
            {
                response.Successful = false;
                response.Error = "Invalid Token";
                response.ResponseSent = DateTime.Now;
                return Json(response, JsonRequestBehavior.AllowGet);
            }

            using (Data.Transaction trans = Data.Transaction.Create(true))
            {
                try
                {
                    if (!VerifyToken(trans, token))
                    {
                        response.Successful = false;
                        response.Error = "Invalid Token";
                        response.ResponseSent = DateTime.Now;
                        return Json(response, JsonRequestBehavior.AllowGet);
                    }

                    response.Successful = true;
                    response.Package = Data.Matters.Matter.Get(trans, matterId);
                }
                catch
                {
                    trans.Rollback();
                    response.Successful = false;
                    response.Package = null;
                    response.Error = "Unexpected server error.";
                }
            }

            response.ResponseSent = DateTime.Now;
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        public ActionResult GetAsset(Guid assetId)
        {
            Guid token;
            Common.Net.Response<Common.Models.Assets.Asset> response
                = new Common.Net.Response<Common.Models.Assets.Asset>();

            response.RequestReceived = DateTime.Now;

            if ((token = GetToken(Request)) == Guid.Empty)
            {
                response.Successful = false;
                response.Error = "Invalid Token";
                response.ResponseSent = DateTime.Now;
                return Json(response, JsonRequestBehavior.AllowGet);
            }

            using (Data.Transaction trans = Data.Transaction.Create(true))
            {
                try
                {
                    if (!VerifyToken(trans, token))
                    {
                        response.Successful = false;
                        response.Error = "Invalid Token";
                        response.ResponseSent = DateTime.Now;
                        return Json(response, JsonRequestBehavior.AllowGet);
                    }

                    response.Successful = true;
                    response.Package = Data.Assets.Asset.Get(trans, assetId);
                }
                catch
                {
                    trans.Rollback();
                    response.Successful = false;
                    response.Package = null;
                    response.Error = "Unexpected server error.";
                }
            }

            response.ResponseSent = DateTime.Now;
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        public ActionResult GetFile(Guid fileId)
        {
            Guid token;
            Common.Net.Response<Common.Models.Assets.File> response
                = new Common.Net.Response<Common.Models.Assets.File>();

            response.RequestReceived = DateTime.Now;

            if ((token = GetToken(Request)) == Guid.Empty)
            {
                response.Successful = false;
                response.Error = "Invalid Token";
                response.ResponseSent = DateTime.Now;
                return Json(response, JsonRequestBehavior.AllowGet);
            }

            using (Data.Transaction trans = Data.Transaction.Create(true))
            {
                try
                {
                    if (!VerifyToken(trans, token))
                    {
                        response.Successful = false;
                        response.Error = "Invalid Token";
                        response.ResponseSent = DateTime.Now;
                        return Json(response, JsonRequestBehavior.AllowGet);
                    }

                    response.Successful = true;
                    response.Package = Data.Assets.File.Get(trans, fileId);
                }
                catch
                {
                    trans.Rollback();
                    response.Successful = false;
                    response.Package = null;
                    response.Error = "Unexpected server error.";
                }
            }

            response.ResponseSent = DateTime.Now;
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        public ActionResult ListCourtTypes()
        {
            Guid token;
            Common.Net.Response<List<Common.Models.Matters.CourtType>> response
                = new Common.Net.Response<List<Common.Models.Matters.CourtType>>();

            response.RequestReceived = DateTime.Now;

            if ((token = GetToken(Request)) == Guid.Empty)
            {
                response.Successful = false;
                response.Error = "Invalid Token";
                response.ResponseSent = DateTime.Now;
                return Json(response, JsonRequestBehavior.AllowGet);
            }

            using (Data.Transaction trans = Data.Transaction.Create(true))
            {
                try
                {
                    if (!VerifyToken(trans, token))
                    {
                        response.Successful = false;
                        response.Error = "Invalid Token";
                        response.ResponseSent = DateTime.Now;
                        return Json(response, JsonRequestBehavior.AllowGet);
                    }

                    response.Successful = true;
                    response.Package = Data.Matters.CourtType.List(trans);
                }
                catch
                {
                    trans.Rollback();
                    response.Successful = false;
                    response.Package = null;
                    response.Error = "Unexpected server error.";
                }
            }

            response.ResponseSent = DateTime.Now;
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        public ActionResult ListCourtGeographicalJurisdictions()
        {
            Guid token;
            Common.Net.Response<List<Common.Models.Matters.CourtGeographicalJurisdiction>> response
                = new Common.Net.Response<List<Common.Models.Matters.CourtGeographicalJurisdiction>>();

            response.RequestReceived = DateTime.Now;

            if ((token = GetToken(Request)) == Guid.Empty)
            {
                response.Successful = false;
                response.Error = "Invalid Token";
                response.ResponseSent = DateTime.Now;
                return Json(response, JsonRequestBehavior.AllowGet);
            }

            using (Data.Transaction trans = Data.Transaction.Create(true))
            {
                try
                {
                    if (!VerifyToken(trans, token))
                    {
                        response.Successful = false;
                        response.Error = "Invalid Token";
                        response.ResponseSent = DateTime.Now;
                        return Json(response, JsonRequestBehavior.AllowGet);
                    }

                    response.Successful = true;
                    response.Package = Data.Matters.CourtGeographicalJurisdiction.List(trans);
                }
                catch
                {
                    trans.Rollback();
                    response.Successful = false;
                    response.Package = null;
                    response.Error = "Unexpected server error.";
                }
            }

            response.ResponseSent = DateTime.Now;
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        public ActionResult ListAssetsForMatter(Guid matterId)
        {
            Guid token;
            Common.Net.Response<List<Common.Models.Assets.Asset>> response
                = new Common.Net.Response<List<Common.Models.Assets.Asset>>();

            response.RequestReceived = DateTime.Now;

            if ((token = GetToken(Request)) == Guid.Empty)
            {
                response.Successful = false;
                response.Error = "Invalid Token";
                response.ResponseSent = DateTime.Now;
                return Json(response, JsonRequestBehavior.AllowGet);
            }

            using (Data.Transaction trans = Data.Transaction.Create(true))
            {
                try
                {
                    if (!VerifyToken(trans, token))
                    {
                        response.Successful = false;
                        response.Error = "Invalid Token";
                        response.ResponseSent = DateTime.Now;
                        return Json(response, JsonRequestBehavior.AllowGet);
                    }

                    response.Successful = true;
                    response.Package = Data.Assets.Asset.ListForMatter(trans, matterId);
                }
                catch
                {
                    trans.Rollback();
                    response.Successful = false;
                    response.Package = null;
                    response.Error = "Unexpected server error.";
                }
            }

            response.ResponseSent = DateTime.Now;
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        public ActionResult ListFilesForAsset(Guid assetId)
        {
            Guid token;
            Common.Net.Response<List<Common.Models.Assets.File>> response
                = new Common.Net.Response<List<Common.Models.Assets.File>>();

            response.RequestReceived = DateTime.Now;

            if ((token = GetToken(Request)) == Guid.Empty)
            {
                response.Successful = false;
                response.Error = "Invalid Token";
                response.ResponseSent = DateTime.Now;
                return Json(response, JsonRequestBehavior.AllowGet);
            }

            using (Data.Transaction trans = Data.Transaction.Create(true))
            {
                try
                {
                    if (!VerifyToken(trans, token))
                    {
                        response.Successful = false;
                        response.Error = "Invalid Token";
                        response.ResponseSent = DateTime.Now;
                        return Json(response, JsonRequestBehavior.AllowGet);
                    }

                    response.Successful = true;
                    response.Package = Data.Assets.Asset.ListFilesForMostRecentVersion(trans, assetId);
                }
                catch
                {
                    trans.Rollback();
                    response.Successful = false;
                    response.Package = null;
                    response.Error = "Unexpected server error.";
                }
            }

            response.ResponseSent = DateTime.Now;
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        public FileResult DownloadFile(Guid id)
        {
            Guid token;
            Common.Models.Assets.File file;

            if ((token = GetToken(Request)) == Guid.Empty)
            {
                return null;
            }

            using (Data.Transaction trans = Data.Transaction.Create(true))
            {
                try
                {
                    if (!VerifyToken(trans, token))
                    {
                        return null;
                    }
                    
                    file = Data.Assets.File.Get(trans, id);
                    file.Version = Data.Assets.Version.Get(trans, file.Version.Id.Value);
                    file.Version.Asset = Data.Assets.Asset.Get(trans, file.Version.Asset.Id.Value);
                }
                catch
                {
                    trans.Rollback();
                    return null;
                }
            }

            Common.FileSystem.Asset fsAsset = new Common.FileSystem.Asset(file.Version.Asset);
            Common.FileSystem.Version fsVersion = new Common.FileSystem.Version(fsAsset, file.Version);
            Common.FileSystem.File fsFile = new Common.FileSystem.File(fsAsset, fsVersion, file);

            return File(fsFile.Path, file.ContentType.ToValue(), file.Version.Asset.Title + file.Extension);
        }

        public ActionResult GetFormDataForMatter(Data.Transaction trans, Guid id)
        {
            Guid token;
            Common.Net.Response<List<Common.Models.Forms.FormFieldMatter>> response
                = new Common.Net.Response<List<Common.Models.Forms.FormFieldMatter>>();

            response.RequestReceived = DateTime.Now;

            if ((token = GetToken(Request)) == Guid.Empty)
            {
                response.Successful = false;
                response.Error = "Invalid Token";
                response.ResponseSent = DateTime.Now;
                return Json(response, JsonRequestBehavior.AllowGet);
            }

            if (!VerifyToken(trans, token))
            {
                response.Successful = false;
                response.Error = "Invalid Token";
                response.ResponseSent = DateTime.Now;
                return Json(response, JsonRequestBehavior.AllowGet);
            }

            response.Successful = true;
            response.Package = Data.Forms.FormFieldMatter.ListForMatter(id);
            response.ResponseSent = DateTime.Now;
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        private Guid GetToken(HttpRequestBase request)
        {
            Guid token;

            if (request.Cookies["Token"] == null)
                return Guid.Empty;

            if (!Guid.TryParse(Request.Cookies["Token"].Value, out token))
                return Guid.Empty;

            return token;
        }

        private bool VerifyToken(Data.Transaction trans, Guid token, bool renewSession = true)
        {
            Common.Models.External.ExternalSession session = Data.External.ExternalSession.Get(trans, token);

            if (session == null) return false;

            if (session.Expires < DateTime.Now) return false;

            if (renewSession)
                session = Data.External.ExternalSession.Renew(trans, session);

            return true;
        }
    }
}
