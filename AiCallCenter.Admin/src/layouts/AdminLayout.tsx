import {
    useState,
    type ReactNode
} from "react";

import {
    AppBar,
    Avatar,
    Box,
    Divider,
    Drawer,
    IconButton,
    List,
    ListItemButton,
    ListItemIcon,
    ListItemText,
    Toolbar,
    Typography
} from "@mui/material";

import {
    AddIcCallOutlined,
    CallOutlined,
    Chat,
    Dashboard,
    HeadsetMicOutlined,
    Menu as MenuIcon,
    MenuBook,
    People,
    Settings,
    SmartToy,
    SupportAgent,
    SupportAgentOutlined
} from "@mui/icons-material";

import {
    useLocation,
    useNavigate
} from "react-router-dom";

const drawerWidth = 250;

interface AdminLayoutProps {
    children: ReactNode;
}

interface StoredUser {
    fullName?: string;
    email?: string;
}

const menuItems = [
    { title: "Kontrol Paneli", path: "/dashboard", icon: <Dashboard /> },
    { title: "AI Ajanları", path: "/ai-agents", icon: <SmartToy /> },
    { title: "Bilgi Bankaları", path: "/knowledge-bases", icon: <MenuBook /> },
    { title: "Sohbetler", path: "/conversations", icon: <Chat /> },
    { title: "Çağrı Operatörleri", path: "/call-operators", icon: <SupportAgent /> },
    { title: "Operatör Paneli", path: "/operator-panel", icon: <SupportAgentOutlined /> },
    { title: "Çağrı Kuyrukları", path: "/call-queues", icon: <HeadsetMicOutlined /> },
    { title: "Yeni Çağrı", path: "/create-call", icon: <AddIcCallOutlined /> },
    { title: "Canlı Çağrılar", path: "/live-calls", icon: <CallOutlined /> },
    { title: "Kullanıcılar", path: "/users", icon: <People /> },
    { title: "Ayarlar", path: "/settings", icon: <Settings /> }
];

function AdminLayout({
    children
}: AdminLayoutProps) {
    const [mobileOpen, setMobileOpen] =
        useState(false);

    const navigate = useNavigate();
    const location = useLocation();

    const storedUser =
        localStorage.getItem("user");

    let user: StoredUser = {};

    if (storedUser) {
        try {
            user = JSON.parse(
                storedUser
            ) as StoredUser;
        } catch {
            user = {};
        }
    }

    const drawerContent = (
        <Box>
            <Toolbar>
                <Typography
                    component="div"
                    variant="h6"
                    sx={{ fontWeight: 700 }}
                >
                    AI Call Center
                </Typography>
            </Toolbar>

            <Divider />

            <List>
                {menuItems.map((item) => (
                    <ListItemButton
                        key={item.path}
                        selected={
                            location.pathname ===
                            item.path
                        }
                        onClick={() => {
                            navigate(item.path);
                            setMobileOpen(false);
                        }}
                    >
                        <ListItemIcon>
                            {item.icon}
                        </ListItemIcon>

                        <ListItemText
                            primary={item.title}
                        />
                    </ListItemButton>
                ))}
            </List>
        </Box>
    );

    return (
        <Box
            sx={{
                display: "flex",
                minHeight: "100vh"
            }}
        >
            <AppBar
                position="fixed"
                sx={{
                    zIndex: (theme) =>
                        theme.zIndex.drawer + 1
                }}
            >
                <Toolbar>
                    <IconButton
                        color="inherit"
                        edge="start"
                        onClick={() =>
                            setMobileOpen(true)
                        }
                        sx={{
                            marginRight: 2,
                            display: {
                                md: "none"
                            }
                        }}
                    >
                        <MenuIcon />
                    </IconButton>

                    <Box sx={{ flexGrow: 1 }} />

                    <Box
                        sx={{
                            display: "flex",
                            alignItems: "center",
                            gap: 1.5
                        }}
                    >
                        <Box
                            sx={{
                                textAlign: "right",
                                display: {
                                    xs: "none",
                                    sm: "block"
                                }
                            }}
                        >
                            <Typography
                                component="div"
                                variant="body2"
                                sx={{ fontWeight: 600 }}
                            >
                                {user.fullName ?? "Kullanıcı"}
                            </Typography>

                            <Typography
                                component="div"
                                variant="caption"
                            >
                                {user.email ?? ""}
                            </Typography>
                        </Box>

                        <Avatar>
                            {(user.fullName ?? "K")
                                .charAt(0)
                                .toUpperCase()}
                        </Avatar>
                    </Box>
                </Toolbar>
            </AppBar>

            <Box
                component="nav"
                sx={{
                    width: { md: drawerWidth },
                    flexShrink: { md: 0 }
                }}
            >
                <Drawer
                    variant="temporary"
                    open={mobileOpen}
                    onClose={() =>
                        setMobileOpen(false)
                    }
                    ModalProps={{ keepMounted: true }}
                    sx={{
                        display: {
                            xs: "block",
                            md: "none"
                        },
                        "& .MuiDrawer-paper": {
                            width: drawerWidth
                        }
                    }}
                >
                    {drawerContent}
                </Drawer>

                <Drawer
                    variant="permanent"
                    sx={{
                        display: {
                            xs: "none",
                            md: "block"
                        },
                        "& .MuiDrawer-paper": {
                            width: drawerWidth,
                            boxSizing: "border-box"
                        }
                    }}
                    open
                >
                    {drawerContent}
                </Drawer>
            </Box>

            <Box
                component="main"
                sx={{
                    flexGrow: 1,
                    padding: 3,
                    width: {
                        md:
                            `calc(100% - ${drawerWidth}px)`
                    },
                    backgroundColor: "#f5f7fb"
                }}
            >
                <Toolbar />
                {children}
            </Box>
        </Box>
    );
}

export default AdminLayout;
